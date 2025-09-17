using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetFilRouge.Models
{
    public class Utilisateur
    {
        public int utilisateurid_pk { get; set; } 

        [Required]
        public string? username { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public required string email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
        public required string password { get; set; }

        public int? roleid { get; set; }
        public Role? role { get; set; }

        public bool emailverified { get; set; }
        
        public string? verificationtoken { get; set; }
        public DateTime dateinscription { get; set; }
    }
}
