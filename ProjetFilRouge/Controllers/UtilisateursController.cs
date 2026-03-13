using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjetFilRouge.Models;

namespace ProjetFilRouge.Controllers
{
    public class UtilisateursController : Controller
    {
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de JeuxController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public UtilisateursController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionCatalogue")!;
            // si la chaîne de connexion'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

 
        public IActionResult AddtoFave(int id)
        {

            // pour avoir des jeux favoris, il faut pouvoir les insérer dans la table correspondante
            // insert into jeux_favoris (jeuid_fk, utilisateurid_fk) VALUES(0, 0);
            // jeuid_pk, utilisateurid_pk sont les identifiants
            // User.ClaimTypes.NameIdentifier, utilisateur.id.ToString()

            try
            {
                var utilisateurId = User.FindFirst(ClaimTypes.NameIdentifier);

                using (var con = new NpgsqlConnection(_connexionString));
                {
                    string queryJeuId = "INSERT INTO jeux_favoris  (jeuid_fk, utilisateurid_fk) VALUES (@jeuid_fk, @utilisateurid_fk)";

                }

            }
            catch (Exception ex) 
            { 

            }



            return View();
        }

        





    }
}

