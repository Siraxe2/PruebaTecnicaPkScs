using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaPkScs.Models;
using PruebaTecnicaPkScs.Services;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;

namespace PruebaTecnicaPkScs.Controllers
{
    public class PokemonController : Controller
    {
        private readonly PokeApiService _pokeApiService;
        private readonly ILogger<PokemonController> _logger;

        private readonly ExcelExportService _excelExportService;

        private readonly EmailService _emailService;

        public PokemonController(
            PokeApiService pokeApiService,
            ILogger<PokemonController> logger,
            ExcelExportService excelExportService,
            EmailService emailService)
        {
            _pokeApiService = pokeApiService;
            _logger = logger;
            _excelExportService = excelExportService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            try
            {
                var pokemon = await _pokeApiService
                    .GetPokemonDetailsAsync(id, cancellationToken);

                if (pokemon is null)
                {
                    return NotFound();
                }

                return View(pokemon);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                _logger.LogError(
                    ex,
                    "Error al consultar los detalles del Pokémon {PokemonId}.",
                    id);

                return StatusCode(
                    503,
                    "No fue posible consultar PokeAPI en este momento.");
            }
            catch (OperationCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "La consulta de detalles del Pokémon {PokemonId} excedio el tiempo de espera.",
                    id);

                return StatusCode(
                    504,
                    "La consulta tardo demasiado. Intentalo nuevamente.");
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Respuesta invalida de PokeAPI para el Pokémon {PokemonId}.",
                    id);

                return StatusCode(
                    502,
                    "No fue posible interpretar la respuesta de PokeAPI.");
            }
        }

        public async Task<IActionResult> Index(
            string? name,
            string? species, 
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            var model = new PokemonIndexViewModel
            {
                Name = name?.Trim() ?? string.Empty,
                SelectedSpecies = species?.Trim() ?? string.Empty
            };

            try
            {
                var catalog = await _pokeApiService
                    .GetPokemonCatalogAsync(cancellationToken);

                var speciesCatalog = await _pokeApiService
                    .GetPokemonSpeciesListAsync(cancellationToken);

                if (catalog is null || speciesCatalog is null)
                {
                    ViewData["Error"] =
                        "La respuesta esta vacia.";

                    return View(model);
                }

                model.SpeciesNames = speciesCatalog.Results
                    .Select(item => item.Name)
                    .OrderBy(name => name)
                    .ToList();

                var query = catalog.Results.AsEnumerable();

                // Aplicar el filtro por nombre.
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    query = query.Where(pokemon =>
                        pokemon.Name.Contains(
                            model.Name,
                            StringComparison.OrdinalIgnoreCase));
                }

                // Aplicar el filtro por especie.
                if (!string.IsNullOrWhiteSpace(model.SelectedSpecies))
                {
                    var canonicalSpeciesName = model.SpeciesNames
                        .FirstOrDefault(item =>
                            item.Equals(
                                model.SelectedSpecies,
                                StringComparison.OrdinalIgnoreCase));

                    if (canonicalSpeciesName is null)
                    {
                        ViewData["Error"] =
                            "La especie seleccionada no es valida.";

                        return View(model);
                    }

                    model.SelectedSpecies = canonicalSpeciesName;

                    var speciesDetails = await _pokeApiService
                        .GetPokemonSpeciesDetailsAsync(
                            canonicalSpeciesName,
                            cancellationToken);

                    if (speciesDetails is null)
                    {
                        ViewData["Error"] =
                            "No fue posible obtener los detalles de la especie.";

                        return View(model);
                    }

                    var varietyNames = speciesDetails.Varieties
                        .Select(variety => variety.Pokemon.Name)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    query = query.Where(pokemon =>
                        varietyNames.Contains(pokemon.Name));
                }

                var matches = query.ToList();

                model.TotalPokemon = catalog.Count;
                model.TotalMatches = matches.Count;

                // Calcular cuàntas paginas hay despues de filtrar.
                model.TotalPages = (int)Math.Ceiling(
                    model.TotalMatches / (double)model.PageSize);

                // Mantener la pagina dentro de los limites validos.
                model.CurrentPage = model.TotalPages == 0
                    ? 1
                    : Math.Min(Math.Max(page, 1), model.TotalPages);

                // Obtener unicamente los registros de la pagina actual.
                int skip = (model.CurrentPage - 1) * model.PageSize;

                model.Pokemon = matches
                    .Skip(skip)
                    .Take(model.PageSize)
                    .ToList();

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex, "Error al conectar con PokeAPI.");

                ViewData["Error"] =
                    "No se pudo conectar con PokeAPI. " +
                    "Verifica tu conexion e intentalo nuevamente.";
            }
            catch (OperationCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex, "La consulta a PokeAPI excedio el tiempo de espera.");

                ViewData["Error"] =
                    "La consulta tardo demasiado. Intentalo nuevamente.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex, "No se pudo interpretar la respuesta de PokeAPI.");

                ViewData["Error"] =
                    "No fue posible interpretar los datos recibidos.";
            }

            return View(model);
        }

        public async Task<IActionResult> ExportExcel(
        string? name,
        string? species,
        int page = 1,
        CancellationToken cancellationToken = default)
        {
            name = name?.Trim();
            species = species?.Trim();

            try
            {
                var catalog = await _pokeApiService
                    .GetPokemonCatalogAsync(cancellationToken);

                if (catalog is null)
                {
                    return StatusCode(
                        502,
                        "No fue posible obtener el catalogo de Pokémon.");
                }

                IEnumerable<PokemonApiItem> query = catalog.Results;

                // Aplicar el filtro por nombre.
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(pokemon =>
                        pokemon.Name.Contains(
                            name,
                            StringComparison.OrdinalIgnoreCase));
                }

                string? canonicalSpeciesName = null;

                // Aplicar el filtro por especie.
                if (!string.IsNullOrWhiteSpace(species))
                {
                    var speciesCatalog = await _pokeApiService
                        .GetPokemonSpeciesListAsync(cancellationToken);

                    canonicalSpeciesName = speciesCatalog?.Results
                        .FirstOrDefault(item =>
                            item.Name.Equals(
                                species,
                                StringComparison.OrdinalIgnoreCase))
                        ?.Name;

                    if (canonicalSpeciesName is null)
                    {
                        return BadRequest(
                            "La especie seleccionada no es valida.");
                    }

                    var speciesDetails = await _pokeApiService
                        .GetPokemonSpeciesDetailsAsync(
                            canonicalSpeciesName,
                            cancellationToken);

                    if (speciesDetails is null)
                    {
                        return StatusCode(
                            502,
                            "No fue posible obtener los detalles de la especie.");
                    }

                    var varietyNames = speciesDetails.Varieties
                        .Select(variety => variety.Pokemon.Name)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    query = query.Where(pokemon =>
                        varietyNames.Contains(pokemon.Name));
                }

                // Obtener todas las coincidencias antes de paginar.
                var matches = query.ToList();

                int pageSize = PokemonIndexViewModel.DefaultPageSize;

                int totalPages = (int)Math.Ceiling(
                    matches.Count / (double)pageSize);

                // Mantener la página dentro del rango válido.
                int currentPage = totalPages == 0
                    ? 1
                    : Math.Min(Math.Max(page, 1), totalPages);

                int skip = (currentPage - 1) * pageSize;

                var visiblePokemon = matches
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                byte[] fileBytes = _excelExportService.ExportPokemon(
                    visiblePokemon,
                    name,
                    canonicalSpeciesName,
                    currentPage,
                    matches.Count);

                string fileName = $"pokemon-pagina-{currentPage}.xlsx";

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener los datos para exportar Pokémon.");

                return StatusCode(
                    503,
                    "No fue posible conectar con PokeAPI.");
            }
            catch (OperationCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "La consulta excedio el tiempo de espera al exportar.");

                return StatusCode(
                    504,
                    "La consulta tardo demasiado. Intentalo nuevamente.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible interpretar los datos para exportar.");

                return StatusCode(
                    502,
                    "La respuesta de PokeAPI no tiene el formato esperado.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPokemonEmail(
        int id,
        string? recipientEmail,
        string? name,
        string? species,
        int page = 1,
        CancellationToken cancellationToken = default)
        {
            if (!IsValidEmail(recipientEmail))
            {
                TempData["EmailError"] =
                    "Introduce una dirección de correo valida.";

                return RedirectToAction(
                    nameof(Index),
                    new { name, species, page });
            }

            try
            {
                var catalog = await _pokeApiService
                    .GetPokemonCatalogAsync(cancellationToken);

                var pokemon = catalog?.Results
                    .FirstOrDefault(item => item.Id == id);

                if (pokemon is null)
                {
                    TempData["EmailError"] =
                        "No se encontro el Pokémon que intentas enviar.";

                    return RedirectToAction(
                        nameof(Index),
                        new { name, species, page });
                }

                await _emailService.SendPokemonEmailAsync(
                    recipientEmail.Trim(),
                    new[] { pokemon },
                    name,
                    species,
                    page,
                    cancellationToken);

                TempData["EmailSuccess"] =
                    $"Se envio el correo con la información de {pokemon.Name}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo enviar el correo del Pokémon {PokemonId}.",
                    id);

                TempData["EmailError"] =
                    "No se pudo enviar el correo. " +
                    "Verifica la configuración SMTP y vuelve a intentarlo.";
            }

            return RedirectToAction(
                nameof(Index),
                new { name, species, page });
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var address = new System.Net.Mail.MailAddress(email.Trim());

                return address.Address.Equals(
                    email.Trim(),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPageEmail(
        string? recipientEmail,
        string? name,
        string? species,
        int page = 1,
        CancellationToken cancellationToken = default)
        {
            name = name?.Trim();
            species = species?.Trim();

            if (!IsValidEmail(recipientEmail))
            {
                TempData["EmailError"] =
                    "Introduce una dirección de correo valida.";

                return RedirectToAction(
                    nameof(Index),
                    new { name, species, page });
            }

            try
            {
                var catalog = await _pokeApiService
                    .GetPokemonCatalogAsync(cancellationToken);

                if (catalog is null)
                {
                    TempData["EmailError"] =
                        "No fue posible obtener el catalogo de Pokémon.";

                    return RedirectToAction(nameof(Index), new { name, species, page });
                }

                IEnumerable<PokemonApiItem> query = catalog.Results;

                // Aplicar el filtro por nombre.
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(pokemon =>
                        pokemon.Name.Contains(
                            name,
                            StringComparison.OrdinalIgnoreCase));
                }

                string? canonicalSpeciesName = null;

                // Aplicar el filtro por especie.
                if (!string.IsNullOrWhiteSpace(species))
                {
                    var speciesCatalog = await _pokeApiService
                        .GetPokemonSpeciesListAsync(cancellationToken);

                    canonicalSpeciesName = speciesCatalog?.Results
                        .FirstOrDefault(item =>
                            item.Name.Equals(
                                species,
                                StringComparison.OrdinalIgnoreCase))
                        ?.Name;

                    if (canonicalSpeciesName is null)
                    {
                        TempData["EmailError"] =
                            "La especie seleccionada no es valida.";

                        return RedirectToAction(
                            nameof(Index),
                            new { name, page });
                    }

                    var speciesDetails = await _pokeApiService
                        .GetPokemonSpeciesDetailsAsync(
                            canonicalSpeciesName,
                            cancellationToken);

                    if (speciesDetails is null)
                    {
                        TempData["EmailError"] =
                            "No fue posible obtener los detalles de la especie.";

                        return RedirectToAction(
                            nameof(Index),
                            new { name, species, page });
                    }

                    var varietyNames = speciesDetails.Varieties
                        .Select(variety => variety.Pokemon.Name)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    query = query.Where(pokemon =>
                        varietyNames.Contains(pokemon.Name));
                }

                // Calcular las coincidencias antes de paginar.
                var matches = query.ToList();

                int pageSize = PokemonIndexViewModel.DefaultPageSize;

                int totalPages = (int)Math.Ceiling(
                    matches.Count / (double)pageSize);

                int currentPage = totalPages == 0
                    ? 1
                    : Math.Min(Math.Max(page, 1), totalPages);

                // Recuperar únicamente la página que se está mostrando.
                var visiblePokemon = matches
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (visiblePokemon.Count == 0)
                {
                    TempData["EmailError"] =
                        "No hay Pokémon en esta pagina para enviar.";

                    return RedirectToAction(
                        nameof(Index),
                        new { name, species = canonicalSpeciesName, page = currentPage });
                }

                await _emailService.SendPokemonEmailAsync(
                    recipientEmail.Trim(),
                    visiblePokemon,
                    name,
                    canonicalSpeciesName,
                    currentPage,
                    cancellationToken);

                TempData["EmailSuccess"] =
                    $"Se envio un correo con {visiblePokemon.Count} Pokémones " +
                    $"de la pagina {currentPage}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo enviar el correo con la pagina de Pokémon.");

                TempData["EmailError"] =
                    "No se pudo enviar el correo. " +
                    "Verifica la configuración SMTP y vuelve a intentarlo.";
            }

            return RedirectToAction(
                nameof(Index),
                new { name, species, page });
        }
    }
}
