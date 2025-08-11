using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Categorie
    {
        public int CategorieId { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [StringLength(90,ErrorMessage ="Le nom ne doit pas dépasser 90 caractères.")]
        public string? Nom { get; set; }

        [StringLength(120,ErrorMessage ="La description ne doit pas dépasser 120 caractères.")]
        public string? Description { get; set; }
    }
}
