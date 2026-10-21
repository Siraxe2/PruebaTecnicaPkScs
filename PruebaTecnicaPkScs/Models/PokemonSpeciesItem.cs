using System.Text.Json.Serialization;

namespace PruebaTecnicaPkScs.Models
{
    public class PokemonSpeciesItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
