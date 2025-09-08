using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Utilisateur
    {
        public int UtilisateurId { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        public required string Nom { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [EmailAddress(ErrorMessage ="Adresse e-mail invalide.")]
        public required string Email { get; set; }

        [Display(Name ="Mot de passe")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [StringLength(10, ErrorMessage = "Le mot de passe doit faire un maximum de 10 caractères.")]
        public required string MotDePasse { get; set; }

        [Display(Name = "Inscription : ")]
        [DataType(DataType.Date)]
        public DateTime DateInscription { get; set; }

        public bool Admin { get; set; }
    }
}
