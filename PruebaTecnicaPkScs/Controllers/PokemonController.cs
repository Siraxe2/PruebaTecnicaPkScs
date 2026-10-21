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
            CancellationToken cancellationToken)
        {
            var model = new PokemonIndexViewModel
            {
                Name = name?.Trim() ?? string.Empty
            };

            try
            {
                var catalog = await _pokeApiService
                    .GetPokemonCatalogAsync(cancellationToken);

                if (catalog is null)
                {
                    ViewData["Error"] =
                        "La respuesta esta vacia.";

                    return View(model);
                }

                var query = catalog.Results.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    query = query.Where(pokemon =>
                        pokemon.Name.Contains(
                            model.Name,
                            StringComparison.OrdinalIgnoreCase));
                }

                var matches = query.ToList();

                model.TotalPokemon = catalog.Count;
                model.TotalMatches = matches.Count;

                // La paginacion manual se implementara despues.
                model.Pokemon = matches.Take(20).ToList();

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
