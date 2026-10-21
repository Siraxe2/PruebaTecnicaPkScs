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

        public PokemonController(
            PokeApiService pokeApiService,
            ILogger<PokemonController> logger)
        {
            _pokeApiService = pokeApiService;
            _logger = logger;
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
    }
}
