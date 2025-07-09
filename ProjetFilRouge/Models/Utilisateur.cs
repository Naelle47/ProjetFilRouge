namespace ProjetFilRouge.Models
{
    public class Utilisateur
    {
        public int UtilisateurId { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public string? MotDePasse { get; set; }
        public DateTime DateInscription { get; set; }
        public bool Admin { get; set; }
    }
}
