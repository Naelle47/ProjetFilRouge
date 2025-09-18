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
        [ValidateAntiForgeryToken]
        public IActionResult SignUp([FromForm] Utilisateur utilisateur)
        {
            // Validation côté serveur
            if (!ModelState.IsValid)
            {
                return View();
            }

            string query = @"INSERT INTO utilisateurs (username, email, password, verificationtoken, roleid_fk) VALUES(@username, @email, @password, @verificationtoken,2)";

            // Hachage du mot de passe
            string motDePasseHache = BC.HashPassword(utilisateur.password);

            // Génération du token de vérification
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int res = connexion.Execute(query, new
                    {
                        username = utilisateur.username,
                        email = utilisateur.email,
                        password = motDePasseHache,
                        verificationtoken = token,
                    });

                    if (res != 1)
                    {
                        throw new Exception("Erreur pendant l'inscription, essayez plus tard.");
                    }

                    // Construction de l'URL de vérification
                    UriBuilder uriBuilder = new UriBuilder
                    {
                        Port = 5106,
                        Path = "/access/verifyemail",
                        Query = $"email={HttpUtility.UrlEncode(utilisateur.email)}&token={HttpUtility.UrlEncode(token)}"
                    };

                    // Envoi du mail
                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress("app@nivo.fr");
                        mail.To.Add(new MailAddress(utilisateur.email));
                        mail.Subject = "Vérification d'email";
                        mail.Body = $"Bonjour {utilisateur.username},<br/><br/>" +
                                    $"Cliquez sur le lien suivant pour vérifier votre email :<br/>" +
                                    $"<a href=\"{uriBuilder.Uri}\">Vérifier Email</a><br/><br/>Merci!";
                        mail.IsBodyHtml = true;

                        using (var smtp = new SmtpClient("localhost", 587))
                        {
                            smtp.Credentials = new NetworkCredential("app@nivo.fr", "fromage");
                            smtp.EnableSsl = false; // à changer en production
                            smtp.Send(mail);
                        }
                    }

                    return RedirectToAction("SignIn");
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
                    int res = connexion.Execute(query, new { email, token });
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([FromForm] Utilisateur utilisateur)
        {
            // TODO vérifier model
            // TODO enlever l'étoile
            string query = "SELECT * FROM utilisateurs JOIN Roles on roleid_fk=roles.id WHERE email=@email AND emailverified=true";

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
                    new { utilisateur.email },
                    splitOn: "id"
                    ).ToList();
                    userFromBDD = users.First();
                }

                // vérifier le mot de passe
                if (BC.Verify(utilisateur.password, userFromBDD.password))
                {
                    // création des revendications de l'utilisateur
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Email, userFromBDD.email),
                        new Claim(ClaimTypes.NameIdentifier, userFromBDD.utilisateurid_pk.ToString()),
                        new Claim(ClaimTypes.Name, userFromBDD.username),
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
                    return RedirectToAction("Index", "Home");
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
