using System.Text.Json.Serialization;

namespace PruebaTecnicaPkScs.Models
{
    public class PokemonSpeciesDetails
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("varieties")]
        public List<PokemonSpeciesVariety> Varieties { get; set; } = new();
    }

    public class PokemonSpeciesVariety
    {
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("pokemon")]
        public PokemonApiItem Pokemon { get; set; } = new();
    }
}
