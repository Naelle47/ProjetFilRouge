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
            // si la chaîne de connexion'a pas été trouvé => déclenche une exception => code http 500 retourné
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
                    jeu = connexion.QuerySingle<Jeu>(query, new { identifiant = id }); // {identifiant = id} -> objet anonyme entre accolades.
                }
                catch (SystemException)
                {
                    return NotFound();
                }
            }
            return View(jeu);
        }


        [HttpGet]
        public IActionResult Nouveau()
        {
            return View();
        }


        // Formulaire pour l'ajout d'un nouveau Jeu.
        [HttpPost]
        public IActionResult Nouveau([FromForm] Jeu jeu)
        {

            string? cheminAbsoluMem = null; // Fichier de mémorisation pour suppression éventuelle post-traitement.

            // Try-catch global pour pouvoir nettoyer la BDD si malgré la réussite de la transaction le processus échoue.

            try
            {
                // Vérifier si le modèle reçu est valide et conforme aux besoins du controlleur, sinon renvoyer la view.
                if (!ModelState.IsValid)
                {
                    return View(jeu);
                }

                // TODO : Vérifier que le jeu n'existe pas déjà avant de continuer, si c'est le cas, gérer l'exception.

                // First step : valider la présence et la viabilité des fichiers reçus.

                bool isValid = true;

                string[] extensionsPermises = { ".jpeg", ".jpg", ".png", ".gif", ".webp" };


                // On regarde si une image a été uploadée, si oui, on vérifie sa viabilité, sinon osef.
                // On récupière le fichier image, on pointe sur son nom dans l'arborescence exploreur et on vérifie que l'extension est valide.

                if (jeu.ImageFile != null || jeu.ImageFile.Length != 0) // Fichier null ou taille zéro = osef, sinon...
                {
                    var extension = Path.GetExtension((jeu.ImageFile).FileName.ToLowerInvariant());

                    if (string.IsNullOrEmpty(extension) || !extensionsPermises.Contains(extension))
                    {
                        isValid = false;
                    }
                }

                if (!isValid)
                {
                    throw new Exception("Ton message d'erreur pour fichier invalide");
                }

                // On génère l'URL puisl le chemin absolu de stockage randomisée pour le fichier image à insérer dans la base de données.

                string cheminRacine = "/pfr_images/jeux/";
                string titreRandomise = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension((jeu.ImageFile).FileName);
                string cheminRelatif = cheminRacine + titreRandomise;
                string cheminAbsolu = "wwwroot" + cheminRelatif;
                cheminAbsoluMem = cheminAbsolu;

                // On vérifie que le fichier de stockage existe, important si plus tard chaque jeu recçoit un sous-fichier.

                Directory.CreateDirectory(Path.GetDirectoryName(cheminAbsolu)!);

                // Si tout va bien, on injecte le fichier wwwroot du site.

                using (var stream = System.IO.File.Create(cheminAbsolu))
                {
                    jeu.ImageFile.CopyTo(stream);
                }

                // Maintenant il faut uploader le fichier dans la BDD via une transaction avec possibilité de rollback.

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    connexion.Open();
                    using (var transaction = connexion.BeginTransaction())
                    {
                        string query = @"INSERT INTO jeux (titre, description, nombrejoueursrecommandes, tempsjeumoyen, dateajout, image) VALUES (@Titre,@Description,@NombreJoueursRecommandes,@TempsJeuMoyen,@DateAjout,@Image)";

                        int res = connexion.Execute(query, jeu, transaction);

                        // Si la transaction échoue (res de valeur 0) alors on rollback + exception.

                        if (res == 0)
                        {
                            transaction.Rollback();
                            throw new Exception("Ton message d'erreur d'insertion de fichier dans la BDD");
                        }

                        // Si la transaction réussie, on commit.

                        transaction.Commit();
                    }
                }
                ViewData["ValidateMessage"] = "Jeu bien créé !";
                return View();
            }

            // Le catch se charge de nettoyer la BDD si le processus échoue pour des raisons mystérieuses™ afin d'éviter les fichiers images orphelins.

            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(cheminAbsoluMem) && System.IO.File.Exists(cheminAbsoluMem))
                {
                    System.IO.File.Delete(cheminAbsoluMem);
                }

                ViewData["ValidateMessage"] = "Erreur";
                return View();
            }
        }
    }
}
