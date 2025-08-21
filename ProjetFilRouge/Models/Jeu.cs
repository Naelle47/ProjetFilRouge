using System.ComponentModel.DataAnnotations;
namespace ProjetFilRouge.Models
{
    public class Jeu
    {

        public int JeuId { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        public string? Titre { get; set; }

        [Required(ErrorMessage ="Le champ est obligatoire.")]
        [StringLength(120,ErrorMessage ="La description ne doit pas dépasser 120 caractères.")]
        public string? Description { get; set; }

        public string? Image { get; set; }
        public int NombreJoueursRecommandes { get; set; }
        public int TempsJeuMoyen { get; set; }

        [Display(Name ="Ajouté le : ")]
        [DataType(DataType.Date)]
        public DateTime DateAjout { get; set; }

        // relations 
        public List<Commentaire> Commentaires { get; set; } = new List<Commentaire>();
        public List<Categorie> Categories { get; set; } = new List<Categorie>();
        public List<Theme> Themes { get; set; } = new List<Theme>();
    }
}
