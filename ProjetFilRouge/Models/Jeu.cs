namespace ProjetFilRouge.Models
{
    public class Jeu
    {
        public int Id { get; set; }
        public string? Titre { get; set; }
        public string? Description { get; set; }
        public DateTime Date_ajout {get; set;}
    }
}
