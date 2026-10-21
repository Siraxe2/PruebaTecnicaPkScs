using Microsoft.Extensions.Caching.Memory;
using PruebaTecnicaPkScs.Models;
using System.Net.Http.Json;

namespace PruebaTecnicaPkScs.Services
{
    public class PokeApiService
    {
        private const string PokemonCatalogCacheKey = "pokemon-catalog";

        private const string PokemonSpeciesCatalogCacheKey = "pokemon-species-catalog";

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public PokeApiService(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }
        
        public async Task<PokemonListResponse?> GetPokemonListAsync(
            int limit = 20,
            int offset = 0,
            CancellationToken cancellationToken = default)
        {
            string endpoint = $"pokemon?limit={limit}&offset={offset}";

            return await _httpClient.GetFromJsonAsync<PokemonListResponse>(
                endpoint,
                cancellationToken);
        }

        public async Task<PokemonListResponse?> GetPokemonCatalogAsync(
        CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(
                    PokemonCatalogCacheKey,
                    out PokemonListResponse? cached)
                && cached is not null)
            {
                return cached;
            }

            // Consultar el total de registros disponibles.
            var summary = await _httpClient
                .GetFromJsonAsync<PokemonListResponse>(
                    "pokemon?limit=1&offset=0",
                    cancellationToken);

            if (summary is null || summary.Count <= 0)
            {
                return summary;
            }

            // Solicitar el catálogo completo utilizando el total reportado.
            var catalog = await _httpClient
                .GetFromJsonAsync<PokemonListResponse>(
                    $"pokemon?limit={summary.Count}&offset=0",
                    cancellationToken);

            if (catalog is not null)
            {
                _memoryCache.Set(
                    PokemonCatalogCacheKey,
                    catalog,
                    TimeSpan.FromMinutes(30));
            }

            return catalog;
        }

        public async Task<PokemonSpeciesListResponse?> GetPokemonSpeciesListAsync(
        CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(
                    PokemonSpeciesCatalogCacheKey,
                    out PokemonSpeciesListResponse? cached)
                && cached is not null)
            {
                return cached;
            }

            // Obtener el total de especies registradas.
            var summary = await _httpClient
                .GetFromJsonAsync<PokemonSpeciesListResponse>(
                    "pokemon-species?limit=1&offset=0",
                    cancellationToken);

            if (summary is null || summary.Count <= 0)
            {
                return summary;
            }

            // Obtener el listado completo para el dropdown.
            var speciesCatalog = await _httpClient
                .GetFromJsonAsync<PokemonSpeciesListResponse>(
                    $"pokemon-species?limit={summary.Count}&offset=0",
                    cancellationToken);

            if (speciesCatalog is not null)
            {
                _memoryCache.Set(
                    PokemonSpeciesCatalogCacheKey,
                    speciesCatalog,
                    TimeSpan.FromMinutes(30));
            }

            return speciesCatalog;
        }

        public async Task<PokemonSpeciesDetails?> GetPokemonSpeciesDetailsAsync(
            string speciesName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(speciesName))
            {
                return null;
            }

            string cacheKey =
                $"pokemon-species-details-{speciesName.ToLowerInvariant()}";

            if (_memoryCache.TryGetValue(
                    cacheKey,
                    out PokemonSpeciesDetails? cached)
                && cached is not null)
            {
                return cached;
            }

            string endpoint =
                $"pokemon-species/{Uri.EscapeDataString(speciesName)}/";

            var details = await _httpClient
                .GetFromJsonAsync<PokemonSpeciesDetails>(
                    endpoint,
                    cancellationToken);

            if (details is not null)
            {
                _memoryCache.Set(
                    cacheKey,
                    details,
                    TimeSpan.FromMinutes(30));
            }

            return details;
        }

        public async Task<PokemonDetailResponse?> GetPokemonDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return null;
            }

            string cacheKey = $"pokemon-details-{id}";

            if (_memoryCache.TryGetValue(
                    cacheKey,
                    out PokemonDetailResponse? cached)
                && cached is not null)
            {
                return cached;
            }

            var details = await _httpClient
                .GetFromJsonAsync<PokemonDetailResponse>(
                    $"pokemon/{id}/",
                    cancellationToken);

            if (details is not null)
            {
                _memoryCache.Set(
                    cacheKey,
                    details,
                    TimeSpan.FromMinutes(30));
            }

            return details;
        }
    }
}
