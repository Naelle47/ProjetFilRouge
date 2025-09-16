using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjetFilRouge.Models;
namespace ProjetFilRouge.Controllers
{
    [Authorize]
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
            const string query = @"
        SELECT 
            j.jeuid_pk AS ""JeuId"", 
            j.titre AS ""Titre"", 
            j.description AS ""Description"", 
            j.image AS ""Image"", 
            j.nombrejoueursrecommandes AS ""NombreJoueursRecommandes"", 
            j.tempsjeumoyen AS ""TempsJeuMoyen"", 
            j.dateajout AS ""DateAjout"",
            c.categorieid_pk AS ""CategorieId"",
            c.nom AS ""Nom"",
            c.description AS ""Description""
        FROM jeux j
        LEFT JOIN jeux_categories jc ON jc.jeuid_fk = j.jeuid_pk
        LEFT JOIN categories c ON jc.categorieid_fk = c.categorieid_pk";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                var jeuxDict = new Dictionary<int, Jeu>();

                var jeux = connexion.Query<Jeu, Categorie, Jeu>(
                    query,
                    (jeu, categorie) =>
                    {
                        if (!jeuxDict.TryGetValue(jeu.JeuId, out var jeuEntry))
                        {
                            jeuEntry = jeu;
                            jeuEntry.Categories = new List<Categorie>();
                            jeuxDict.Add(jeu.JeuId, jeuEntry);
                        }

                        if (categorie != null)
                        {
                            jeuEntry.Categories.Add(categorie);
                        }

                        return jeuEntry;
                    },
                    splitOn: "CategorieId"
                ).Distinct().ToList();

                return View(jeux);
            }
        }




        // afficher le détail d'un jeu -- route paramétrée
        public IActionResult Detail([FromRoute] int id)
        {
            const string query = @"
        SELECT 
            jeuid_pk AS ""JeuId"", 
            titre AS ""Titre"", 
            description AS ""Description"", 
            image AS ""Image"", 
            nombrejoueursrecommandes AS ""NombreJoueursRecommandes"", 
            tempsjeumoyen AS ""TempsJeuMoyen"", 
            dateajout AS ""DateAjout""
        FROM jeux
        WHERE jeuid_pk = @identifiant;

        SELECT 
            c.categorieid_pk AS ""CategorieId"", 
            c.nom AS ""Nom"", 
            c.description AS ""Description""
        FROM categories c
        INNER JOIN jeux_categories jc ON jc.categorieid_fk = c.categorieid_pk
        WHERE jc.jeuid_fk = @identifiant;

        SELECT 
            t.themeid_pk AS ""ThemeId"", 
            t.nom AS ""Nom"", 
            t.description AS ""Description""
        FROM themes t
        INNER JOIN jeux_themes jt ON jt.themeid_fk = t.themeid_pk
        WHERE jt.jeuid_fk = @identifiant;";

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                using (var multi = connexion.QueryMultiple(query, new { identifiant = id }))
                {
                    var jeu = multi.ReadSingle<Jeu>();
                    var categories = multi.Read<Categorie>().ToList();
                    var themes = multi.Read<Theme>().ToList();

                    jeu.Categories = categories;
                    jeu.Themes = themes;

                    return View(jeu);
                }
            }
        }

        // API FETCH - Rechercher les Jeux par Nom
        [HttpGet]
        public IActionResult RechercheJeu([FromQuery] string titre)
        {
            string query = "SELECT * FROM jeux WHERE lower(titre) like lower(@titre)";
            List<Jeu> jeux;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                jeux = connexion.Query<Jeu>(query, new { titre = "%" + titre + "%" }).ToList();
            }
            return Json(jeux);

        }

        // Formulaires pour un Nouveau Jeu
        [HttpGet]
        public IActionResult Nouveau()
        {
            return View();
        }
        // Formulaire pour l'ajout d'un nouveau Jeu.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                // On récupère le fichier image, on pointe sur son nom dans l'arborescence exploreur et on vérifie que l'extension est valide.

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

                // On génère l'URL puis le chemin absolu de stockage randomisée pour le fichier image à insérer dans la base de données.

                string cheminRacine = "/pfr_images/jeux/";
                string titreRandomise = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension((jeu.ImageFile).FileName);
                string cheminRelatif = cheminRacine + titreRandomise;
                string cheminAbsolu = "wwwroot" + cheminRelatif;
                cheminAbsoluMem = cheminAbsolu;

                // On vérifie que le fichier de stockage existe, important si plus tard chaque jeu reçoit un sous-fichier.

                Directory.CreateDirectory(Path.GetDirectoryName(cheminAbsolu)!);

                // Si tout va bien, on injecte le fichier wwwroot du site.

                using (var stream = System.IO.File.Create(cheminAbsolu))
                {
                    jeu.ImageFile.CopyTo(stream);
                }
                jeu.Image = cheminRelatif;
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
