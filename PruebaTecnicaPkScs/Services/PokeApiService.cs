using System.Net.Http.Json;
using PruebaTecnicaPkScs.Models;

namespace PruebaTecnicaPkScs.Services
{
    public class PokeApiService
    {
        private readonly HttpClient _httpClient;

        public PokeApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
    }
}
