namespace PruebaTecnicaPkScs.Models
{
    public class PokemonIndexViewModel
    {
        public const int DefaultPageSize = 20;

        public string Name { get; set; } = string.Empty;

        public string SelectedSpecies { get; set; } = string.Empty;

        public List<string> SpeciesNames { get; set; } = new();

        public int TotalPokemon { get; set; }

        public int TotalMatches { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = DefaultPageSize;

        public int TotalPages { get; set; }

        public List<PokemonApiItem> Pokemon { get; set; } = new();
    }
}
