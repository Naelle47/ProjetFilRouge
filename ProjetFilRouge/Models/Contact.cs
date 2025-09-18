using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Contact
    {
        [Required(ErrorMessage = "Le pseudo est obligatoire.")]
        public string Pseudo { get; set; }

        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "L'objet est obligatoire.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Le message est obligatoire.")]
        [MinLength(10, ErrorMessage = "Le message doit contenir au moins 10 caractères.")]
        public string Message { get; set; }
    }
}
