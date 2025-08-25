using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;
using ProjetFilRouge.Models;
namespace ProjetFilRouge.Controllers
{
    public class JeuxController : Controller 
    {
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public JeuxController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionCatalogue")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        // afficher le catalogue des jeux
        public IActionResult Index()
        {
            string query = @"
            SELECT 
                jeuid_pk AS ""JeuId"", 
                Titre, 
                Description, 
                Image, 
                NombreJoueursRecommandes, 
                TempsJeuMoyen, 
                DateAjout
            FROM Jeux
            ORDER BY ""JeuId""";
            List<Jeu> jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                jeux = connexion.Query<Jeu>(query).ToList();
            }
            return View(jeux);
        }




        // afficher le détail d'un jeu -- route paramétrée
        public IActionResult Detail([FromRoute] int id)
        {

            // construction de la requête SQL
            // déclaration de la variable permettant de contenir le résultat de la requête SQL
            // exécuter la requête SQL
            // retourner la vue contenant le résultat
            string query = @"
            SELECT 
                jeuid_pk AS ""JeuId"", 
                Titre, 
                Description, 
                Image, 
                NombreJoueursRecommandes, 
                TempsJeuMoyen, 
                DateAjout
            FROM Jeux
            WHERE jeuid_pk = @identifiant";
            Jeu jeu;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    jeu = connexion.QuerySingle<Jeu>(query, new {identifiant = id}); // {identifiant = id} -> objet anonyme entre accolades.
                }
                catch (SystemException)
                {
                    return NotFound();
                }
            }
            return View(jeu);
        }

        [HttpPost]
        public IActionResult Create(EditeurJeuxViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si le formulaire n'est pas valide, recharge la liste des jeux
                string query = @"
            SELECT 
                jeuid_pk AS ""JeuId"",
                Titre,
                Description,
                Image,
                NombreJoueursRecommandes,
                TempsJeuMoyen,
                DateAjout
            FROM Jeux
            ORDER BY Titre";

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    model.Jeux = connexion.Query<Jeu>(query).ToList();
                }

                return View("Index", model);
            }

            string insertQuery = @"
        INSERT INTO Jeux (Titre, TempsJeuMoyen, NombreJoueursRecommandes, Categorie, DateSortie)
        VALUES (@Titre, @TempsJeuMoyen, @NombreJoueursRecommandes, @Categorie, @DateSortie)";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Execute(insertQuery, model);
            }

            return RedirectToAction("Index");
        }
    }
}
