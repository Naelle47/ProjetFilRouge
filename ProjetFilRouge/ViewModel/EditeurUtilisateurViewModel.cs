namespace ProjetFilRouge.ViewModel
{
    public class EditeurUtilisateurViewModel
    {
        public int UtilisateurId { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public bool Admin { get; set; } // indique si l'utilisateur est admin
    }
}
