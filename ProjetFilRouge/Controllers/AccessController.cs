using System.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjetFilRouge.Models;
using BC = BCrypt.Net.BCrypt;

namespace ProjetFilRouge.Controllers
{
    public class AccessController : Controller
    {
        private readonly string _connexionString;

        public AccessController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("GestionCatalogue")!;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp([FromForm] Utilisateur utilisateur)
        {
            // TODO valider le modèle

            string query = "INSERT INTO Utilisateurs (username,email,password,verificationtoken) VALUES (@username,@email,@password,@verificationtoken)";

            // hachage du mot de passe
            string motDePasseHache = BC.HashPassword(utilisateur.Password);
            // génération du token de vérification d'adresse mail
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
                        verificationtoken = token
                    });
                    if (res != 1)
                    {
                        throw new Exception("Erreur êndant l'inscription, essai plus tard.");
                    }
                    else
                    {
                        UriBuilder uriBuilder = new UriBuilder();
                        uriBuilder.Port = 5103;
                        uriBuilder.Path = "/access/verifyemail";
                        uriBuilder.Query = $"email={HttpUtility.UrlEncode(utilisateur.Email)}&token={HttpUtility.UrlEncode(token)}";

                        // envoi du mail avec le token
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress("app@kitsune.fr");
                        mail.To.Add(new MailAddress(utilisateur.Email));
                        mail.Subject = "Vérification d'email";
                        mail.Body = $"<a href={uriBuilder.Uri}>Vérifier l'email</a>";
                        mail.IsBodyHtml = true; // permet de dire que le corps du message contient de l'html afin que le client mail affiche le corps du message en html (comme un navigateur)

                        using (var smtp = new SmtpClient("localhost", 587))
                        {
                            smtp.Credentials = new NetworkCredential("app@kitsune.fr", "123456");
                            smtp.EnableSsl = false; // devrait être à true mais l'environnement de test ne le permet pas
                            smtp.Send(mail);
                        }

                        return RedirectToAction("SignIn");
                    }
                }
            }
            catch (Exception e)
            {
                ViewData["ValidateMessage"] = e.Message;  // TODO à ajouter dans la vue
                return View(utilisateur);
            }
        }

        public IActionResult VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            // TODO vérifier qu'on recoit bien des truc
            string query = "UPDATE Utilisateurs SET emailverified=true WHERE email=@email AND verificationtoken=@token";
            try
            {


                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int res = connexion.Execute(query, new { email = email, token = token });
                    if (res != 1)
                    {
                        throw new Exception("Pb pendant la vérif, veuillez recommencer");
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

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] Utilisateur utilisateur)
        {
            // TODO vérifier model
            // TODO enlever l'étoile
            string query = "SELECT * FROM Utilisateurs JOIN Roles on role_id=roles.id WHERE email=@email AND emailverified=true";
            try
            {
                Utilisateur userFromBDD;
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    List<Utilisateur> users = connexion.Query<Utilisateur, Role, Utilisateur>(query, (utilisateur, role) =>
                    {
                        utilisateur.role = role;
                        return utilisateur;
                    }
                    ,
                    new { email = utilisateur.Email },
                    splitOn: "id"
                    ).ToList();
                    userFromBDD = users.First();
                }

                // vérifier le mot de passe
                if (BC.Verify(utilisateur.Password, userFromBDD.Password))
                {
                    // création des revendications de l'utilisateur
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Email, userFromBDD.Email),
                        new Claim(ClaimTypes.NameIdentifier, userFromBDD.UtilisateurId.ToString()),
                        new Claim(ClaimTypes.Name, userFromBDD.Username),
                        new Claim(ClaimTypes.Role, userFromBDD.role.name ),
                     };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // création du cookie
                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                    };

                    // vous aurez besoin de modifier le type de retour de votre méthode en Task<IActionResult> (programmation asynchrone étudiée plus tard dans la formation)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    if (Request.Form.ContainsKey("ReturnUrl"))
                    {
                        return Redirect(Request.Form["ReturnUrl"]!);
                    }
                    return RedirectToAction("Index", "Livres");
                }
                else
                {
                    // TODO gérer les erreurs du model et vider mot de passe
                    return View(utilisateur);
                }
            }
            catch (Exception e)
            {
                // TODO a faire
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
