namespace PruebaTecnicaPkScs.Models
{
    public class PokemonIndexViewModel
    {
        public string Name { get; set; } = string.Empty;

        public int TotalPokemon { get; set; }

        public int TotalMatches { get; set; }

        public List<PokemonApiItem> Pokemon { get; set; } = new();
    }
}
