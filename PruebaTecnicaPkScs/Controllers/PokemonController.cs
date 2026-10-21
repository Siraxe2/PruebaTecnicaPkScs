using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaPkScs.Models;
using PruebaTecnicaPkScs.Services;
using System.Net.Http;
using System.Text.Json;

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
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _pokeApiService
                    .GetPokemonListAsync(
                        cancellationToken: cancellationToken);

                if (response is null)
                {
                    ViewData["Error"] =
                        "La respuesta esta vacia.";

                    return View(new PokemonListResponse());
                }

                return View(response);
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

            return View(new PokemonListResponse());
        }
    }
}
