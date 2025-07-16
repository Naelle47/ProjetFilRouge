using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class Theme
    {
        public int ThemeId { get; set; }

        [Required(ErrorMessage = "Le champ est obligatoire.")]
        [StringLength(50, ErrorMessage = "Le nom ne doit pas dépasser 50 caractères.")]
        public string? Nom { get; set; }

        [StringLength(120, ErrorMessage = "La description ne doit pas dépasser 120 caractères.")]
        public string? Description { get; set; }
    }
}
