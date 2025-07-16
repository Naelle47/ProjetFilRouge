using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Utilisateur
    {
        public int UtilisateurId { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        public string? Nom { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [EmailAddress(ErrorMessage ="Adresse e-mail invalide.")]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Le champ est obligatoire.")]
        public string? MotDePasse { get; set; }

        [Display(Name = "Inscription : ")]
        [DataType(DataType.Date)]
        public DateTime DateInscription { get; set; }

        public bool Admin { get; set; }
    }
}
