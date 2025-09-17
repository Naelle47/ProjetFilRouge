using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Commentaire
    {
        public int jeuid_fk { get; set; }

        public int utilisateurid_fk { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(250,ErrorMessage ="Le commentaire ne doit pas dépasser 250 caractères.")]
        public string commentaire { get; set; }

        [Display(Name ="Posté le : ")]
        [DataType(DataType.Date)]
        public DateTime datecommentaire { get; set; }

        public int note { get; set; }

        public string username { get; set; }
        public string? titre { get; set; }
    }
}
