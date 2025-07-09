namespace ProjetFilRouge.Models
{
    public class Commentaire
    {
        public int JeuId { get; set; }
        public int UtilisateurId { get; set; }
        public string? Texte { get; set; }
        public DateTime DateCommentaire { get; set; }

        public Utilisateur? Utilisateur { get; set; }
    }
}
