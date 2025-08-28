using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjetFilRouge.Models
{
    public class EditeurJeuxViewModel
    {
       public Jeu? jeu {  get; set; }
        public List<SelectListItem> categories { get; set; } = new List<SelectListItem>();
        public required string action { get; init; }
        public required string titre { get; init; }

        public int? JeuId { get; set; } = null;
    }
}
