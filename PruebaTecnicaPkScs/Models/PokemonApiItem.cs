using System.Text.Json.Serialization;

namespace PruebaTecnicaPkScs.Models
{
    public class PokemonApiItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonIgnore]
        public int Id
        {
            get
            {
                //Id calculada apartir del contenido de la url
                string normalizedUrl = Url.TrimEnd('/');
                int lastSlash = normalizedUrl.LastIndexOf('/');

                string idText = lastSlash >= 0
                    ? normalizedUrl[(lastSlash + 1)..]
                    : normalizedUrl;

                return int.TryParse(idText, out int id) ? id : 0;
            }
        }

        [JsonIgnore]
        public string ImageUrl =>
            Id > 0
                ? $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{Id}.png"
                : string.Empty;
    }
}
