namespace PruebaTecnicaPkScs.Models
{
    public class PokemonIndexViewModel
    {
        public string Name { get; set; } = string.Empty;

        public string SelectedSpecies { get; set; } = string.Empty;

        public List<string> SpeciesNames { get; set; } = new();

        public int TotalPokemon { get; set; }

        public int TotalMatches { get; set; }

        public List<PokemonApiItem> Pokemon { get; set; } = new();
    }
}
