using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Utilisateur
    {
        public int utilisateurid { get; set; }  // sera rempli par la DB après insert

        [Required]
        public string username { get; set; }

        [Required, EmailAddress]
        public required string email { get; set; }

        [Required, DataType(DataType.Password)]
        public required string password { get; set; }

        public int? roleid { get; set; }
        public Role? role { get; set; }

        public bool emailverified { get; set; }
        public string verificationtoken { get; set; }
        public DateTime dateinscription { get; set; }
    }
}
