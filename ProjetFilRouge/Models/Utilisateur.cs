using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Utilisateur
    {

        //public Utilisateur(string username, string password, string email)
        //{
        //    this.Username = username;
        //    this.Password = password;
        //    this.Email = email;
        //}

        public int UtilisateurId { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        public required string Username { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [EmailAddress(ErrorMessage ="Adresse e-mail invalide.")]
        public required string Email { get; set; }

        [Display(Name ="Mot de passe")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [StringLength(10, ErrorMessage = "Le mot de passe doit faire un maximum de 10 caractères.")]
        public required string Password { get; set; }

        [Display(Name = "Inscription : ")]
        [DataType(DataType.Date)]
        public DateTime DateInscription { get; set; }

        public bool Admin { get; set; }
        public bool EmailVerified { get; set; }
    }
}
