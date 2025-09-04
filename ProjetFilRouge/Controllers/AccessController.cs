using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Npgsql;
using ProjetFilRouge.Models;
using BC = BCrypt.Net.BCrypt;

namespace ProjetFilRouge.Controllers
{
    
    public class AccessController : Controller
    {
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public AccessController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("GestionCatalogue")!;
            // si la chaîne de connexion'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        // méthode pour l'inscription
        [HttpGet]
        public IActionResult SignUp() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp([FromForm] Utilisateur utilisateur)
        {
            // TODO : Valider le modèle

            string query = "INSERT INTO Utilisateurs (nom, email, motdepasse,verificationtoken) VALUES (@nom, @email, @motdepasse,@verificationtoken)";

            // Hachage du MDP
            string motDePasseHache = BC.HashPassword(utilisateur.MotDePasse);
            // Génération du Token de Vérification de l'adresse Email
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int res = connexion.Execute(query, new
                    {
                        email = utilisateur.Email,
                        password = motDePasseHache,
                        verificationtoken = token,
                    });
                    if (res != 1)
                    {
                        throw new Exception("Erreur pendant l'inscription, essaye plus tard.");
                    }
                    else
                    {
                        UriBuilder uriBuilder = new UriBuilder();
                        uriBuilder.Port = 5103;
                        uriBuilder.Path = "/access/verifyemail";
                        uriBuilder.Query = $"email={HttpUtility.UrlEncode(utilisateur.Email)}&token={HttpUtility.UrlEncode(token)}";

                        // Envoi du mail avec le token
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress("app@nivo.fr");
                        mail.To.Add(new MailAddress(utilisateur.Email));
                        mail.Subject = "Objet de l'email";
                        mail.Body = $"<a href={uriBuilder.Uri}>Vérifier l'email</a>";
                        mail.IsBodyHtml = true; // permet de dire que le corps du message contient de l'html afin que le client mail affiche le corps du message en html (comme un navigateur)

                        using (var smtp = new SmtpClient("localhost", 587))
                        {
                            smtp.Credentials = new NetworkCredential("app@nivo.fr", "mot de passe");
                            smtp.EnableSsl = false; // devrait être à true mais l'environnement de test ne le permet pas
                            smtp.Send(mail);
                        }
                        return RedirectToAction("SignIn");
                    }
                }
            }
            catch (Exception e)
            {
                ViewData["ValidateMessage"] = e.Message; // TODO : à ajouter dans la vue
                return View(utilisateur);
            }


        }
        

    // TO DO : Ajouter la méthode VerifyEmail
    public IActionResult VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            // TODO : Vérifier la réception des infos
            string query = "UPDATE Utilisateurs SET emailverified=true WHERE email=@email AND verificationtoken=@token";
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int res = connexion.Execute(query, new { email = email, token = token });
                    if (res != 1)
                    {
                        throw new Exception("Problème pendant la vérification, veuillez recommencer.");
                    }
                    else
                    {
                        ViewData["ValidateMessage"] = "Email vérifié, vous pouvez maintenant vous connecter.";
                        return View();
                    }
                }
            }
            catch (Exception e)
            {
                ViewData["ValidateMessage"] = e.Message;
                return View();
            }
        }

        // métode pour la connexion
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] Utilisateur utilisateur)
        {
            // TODO : Vérifier le modèle
            string query = "SELECT * from Utilisateurs WHERE email=@email AND emailverified=true";

            try
            {
                Utilisateur userFromBDD;
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    userFromBDD = connexion.QuerySingle<Utilisateur>(query, new { email = utilisateur.Email });
                }

                // Vérifier le MDP
                if (BC.Verify(utilisateur.MotDePasse, userFromBDD.MotDePasse))
                {
                    return RedirectToAction("Index");
                    // création des revendications de l'utilisateur
                    List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Email, userFromBDD.Email),
                            new Claim(ClaimTypes.NameIdentifier, userFromBDD.UtilisateurId.ToString()),
                            new Claim(ClaimTypes.Name, userFromBDD.Nom),
                        };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // création du cookie
                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                    };

                    
                    // vous aurez besoin de modifier le type de retour de votre méthode en Task<IActionResult> (programmation asynchrone étudiée plus tard dans la formation)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    if (Request.Form.ContainsKey("ReturnURL"))
                    {
                        return Redirect(Request.Form["ReturnURL"]!);
                    }
                    return RedirectToAction("Index","Jeux");
                }
                else
                {
                    // TODO : Gérer les erreurs du model et vider le MDP
                    return View(utilisateur);
                }

            }
            catch (Exception e) 
            {
                return View();
            }
        }

        public async Task<IActionResult> SignOut()
        {
            // vous aurez besoin de modifier le type de retour de votre méthode en Task<IActionResult> (programmation asynchrone étudiée plus tard dans la formation)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }
    }
}
