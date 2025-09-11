namespace ProjetFilRouge.Models
{
    public class Role
    {
        public int id { get; set; }
        public string? name { get; set; }

        public List<Utilisateur> utilisateurs { get; set; } = new List<Utilisateur>();
    }
}
