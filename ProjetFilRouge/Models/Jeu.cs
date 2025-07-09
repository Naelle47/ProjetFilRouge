namespace ProjetFilRouge.Models
{
    public class Jeu
    {
        public int JeuId { get; set; }
        public string? Titre { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int NombreJoueursRecommandes { get; set; }
        public int TempsJeuMoyen { get; set; }
        public DateTime DateAjout { get; set; }

        // relations 
        public List<Commentaire> Commentaires { get; set; } = new List<Commentaire>();
        public List<Categorie> Categories { get; set; } = new List<Categorie>();
        public List<Theme> Themes { get; set; } = new List<Theme>();
    }
}
