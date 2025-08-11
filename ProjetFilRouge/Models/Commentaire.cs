using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Commentaire
    {
        public int JeuId { get; set; }



        public int UtilisateurId { get; set; }


        [DataType(DataType.MultilineText)]
        [StringLength(250,ErrorMessage ="Le commentaire ne doit pas dépasser 250 caractères.")]
        public string? Texte { get; set; }

        [Display(Name ="Posté le : ")]
        [DataType(DataType.Date)]
        public DateTime DateCommentaire { get; set; }



        public Utilisateur? Utilisateur { get; set; }
    }
}
