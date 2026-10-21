using System.Text.Json.Serialization;

namespace PruebaTecnicaPkScs.Models
{
    public class PokemonSpeciesListResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<PokemonSpeciesItem> Results { get; set; } = new();
    }
}
