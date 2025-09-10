using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetFilRouge.Models
{
    public class Utilisateur
    {
        public int UtilisateurId { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        public string? Username { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [EmailAddress(ErrorMessage ="Adresse e-mail invalide.")]
        public required string Email { get; set; }

        [Display(Name ="Mot de passe")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit faire entre 6 et 100 caractères.")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [NotMapped]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Inscription : ")]
        [DataType(DataType.Date)]
        public DateTime DateInscription { get; set; } = DateTime.Now;

        public bool Admin { get; set; }
        public bool EmailVerified { get; set; }
    }
}
