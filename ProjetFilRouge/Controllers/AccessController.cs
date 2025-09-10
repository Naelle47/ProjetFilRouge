using System.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using BCrypt.Net;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjetFilRouge.Models;

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
        public IActionResult Authentification(string? activeForm = "SignIn")
        {
            ViewData["ActiveForm"] = activeForm;
            return View();
        }

        // --- SIGN UP ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp([FromForm] Utilisateur utilisateur)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ActiveForm"] = "SignUp";
                return View("Authentification", utilisateur);
            }

            string query = @"INSERT INTO utilisateurs 
                            (username, email, password, dateinscription, admin, emailverified, verificationtoken) 
                            VALUES (@Username, @Email, @PasswordHash, @DateInscription, false, false, @VerificationToken)";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    string motDePasseHache = BCrypt.Net.BCrypt.HashPassword(utilisateur.Password);

                    byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                    byte[] key = Guid.NewGuid().ToByteArray();
                    string token = Convert.ToBase64String(time.Concat(key).ToArray());

                    int res = connexion.Execute(query, new
                    {
                        utilisateur.Username,
                        utilisateur.Email,
                        PasswordHash = motDePasseHache,
                        DateInscription = DateTime.UtcNow,
                        VerificationToken = token
                    });

                    if (res != 1)
                        throw new Exception("Erreur pendant l'inscription, veuillez réessayer plus tard.");

                    // Lien de vérification
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Port = 5106;
                    uriBuilder.Path = "/Access/VerifyEmail";
                    uriBuilder.Query = $"email={HttpUtility.UrlEncode(utilisateur.Email)}&token={HttpUtility.UrlEncode(token)}";

                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("app@nivo.fr");
                    mail.Subject = "Vérification d'email";
                    mail.Body = $"<a href=\"{uriBuilder.Uri}\">Vérifier l'email</a>";
                    mail.IsBodyHtml = true;
                    
                    mail.To.Add(new MailAddress(utilisateur.Email));

                    using (var smtp = new SmtpClient("localhost", 587))
                    {
                        smtp.Credentials = new NetworkCredential("app@nivo.fr", "fromage");
                        smtp.EnableSsl = false; // ⚠️ mettre true en prod
                        smtp.Send(mail);
                    }

                    return RedirectToAction("Authentification", new { activeForm = "SignIn" });
                }
            }
            catch (Exception e)
            {
                ViewData["ActiveForm"] = "SignUp";
                ViewData["ValidateMessage"] = e.Message;
                return View("Authentification", utilisateur);
            }
        }

        // --- VERIFY EMAIL ---
        public IActionResult VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            string query = "UPDATE Utilisateurs SET emailverified=true WHERE email=@Email AND verificationtoken=@Token";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int res = connexion.Execute(query, new { Email = email, Token = token });
                    if (res != 1)
                        throw new Exception("Pb pendant la vérif, veuillez recommencer");

                    ViewData["ValidateMessage"] = "Email vérifié, vous pouvez maintenant vous connecter.";
                    ViewData["ActiveForm"] = "SignIn"; 
                    return View("Authentification");
                }
            }
            catch (Exception e)
            {
                ViewData["ValidateMessage"] = e.Message;
                ViewData["ActiveForm"] = "SignUp"; 
                return View("Authentification");
            }
        }

        // --- SIGN IN ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([FromForm] Utilisateur utilisateur)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ActiveForm"] = "SignIn";
                return View("Authentification", utilisateur);
            }

            string query = @"SELECT * FROM utilisateurs JOIN Roles on roleid_fk = roles.id WHERE email = @Email AND emailverified=true";

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
                    new { email = utilisateur.email },
                    splitOn: "id"
                    ).ToList();
                    userFromBDD = users.First();
                }
            }
            }
            catch (Exception e)
            {
                ViewData["ActiveForm"] = "SignIn";
                ViewData["ValidateMessage"] = e.Message;
                return View("Authentification", utilisateur);
            }
        }

        // --- LOGOUT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Authentification", "Access");
        }
    }
}
