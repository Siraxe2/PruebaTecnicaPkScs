using Microsoft.Extensions.Caching.Memory;
using PruebaTecnicaPkScs.Models;
using System.Net.Http.Json;

namespace PruebaTecnicaPkScs.Services
{
    public class PokeApiService
    {
        private const string PokemonCatalogCacheKey = "pokemon-catalog";

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
    }
}
