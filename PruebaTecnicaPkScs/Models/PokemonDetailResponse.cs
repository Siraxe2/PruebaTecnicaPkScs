using System.Text.Json.Serialization;

namespace PruebaTecnicaPkScs.Models
{
    public class PokemonDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        [JsonPropertyName("sprites")]
        public PokemonDetailSprites Sprites { get; set; } = new();

        [JsonPropertyName("types")]
        public List<PokemonTypeSlot> Types { get; set; } = new();

        public string? ImageUrl =>
            Sprites.Other?.OfficialArtwork?.FrontDefault
            ?? Sprites.FrontDefault;

        public double HeightInMeters => Height / 10.0;

        public double WeightInKilograms => Weight / 10.0;
    }

    public class PokemonDetailSprites
    {
        [JsonPropertyName("front_default")]
        public string? FrontDefault { get; set; }

        [JsonPropertyName("other")]
        public PokemonOtherSprites? Other { get; set; }
    }

    public class PokemonOtherSprites
    {
        [JsonPropertyName("official-artwork")]
        public PokemonOfficialArtwork? OfficialArtwork { get; set; }
    }

    public class PokemonOfficialArtwork
    {
        [JsonPropertyName("front_default")]
        public string? FrontDefault { get; set; }
    }

    public class PokemonTypeSlot
    {
        [JsonPropertyName("type")]
        public PokemonNamedResource Type { get; set; } = new();
    }

    public class PokemonNamedResource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
