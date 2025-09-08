using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using BCrypt.Net;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        // --- SIGN UP ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp([FromForm] Utilisateur utilisateur)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ActiveForm"] = "SignUp";
                return View("Auth", utilisateur);
            }

            string query = @"INSERT INTO ""Utilisateurs"" 
                            (username, email, password, dateinscription, admin, emailverified, verificationtoken) 
                            VALUES (@Username, @Email, @PasswordHash, false, false, @VerificationToken)";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    // Hashage du mot de passe
                    string motDePasseHache = BCrypt.Net.BCrypt.HashPassword(utilisateur.Password);

                    // Génération du token de vérification
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
                    UriBuilder uriBuilder = new UriBuilder
                    {
                        Port = 5103,
                        Path = "/Access/VerifyEmail",
                        Query = $"email={HttpUtility.UrlEncode(utilisateur.Email)}&token={HttpUtility.UrlEncode(token)}"
                    };

                    // Envoi du mail de vérification
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("app@nivo.fr");
                    mail.To.Add(new MailAddress(utilisateur.Email));
                    mail.Subject = "Vérification d'email";
                    mail.Body = $"<a href={uriBuilder.Uri}>Vérifier l'email</a>";
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("localhost", 587))
                    {
                        smtp.Credentials = new NetworkCredential("app@nivo.fr", "fromage");
                        smtp.EnableSsl = false; // ⚠️ doit être true en prod
                        smtp.Send(mail);
                    }

                    return RedirectToAction("Auth", new { activeForm = "SignIn" });
                }
            }
            catch (Exception e)
            {
                ViewData["ActiveForm"] = "SignUp";
                ViewData["ValidateMessage"] = e.Message;
                return View("Auth", utilisateur);
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
                return View("Auth", utilisateur);
            }

            string query = @"SELECT * FROM ""Utilisateurs"" WHERE email = @Email";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    var user = connexion.QueryFirstOrDefault<Utilisateur>(query, new { utilisateur.Email });

                    if (user == null || !BCrypt.Net.BCrypt.Verify(utilisateur.Password, user.Password))
                        throw new Exception("Email ou mot de passe incorrect.");

                    if (!user.EmailVerified)
                        throw new Exception("Veuillez vérifier votre adresse e-mail avant de vous connecter.");

                    // Création des claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User")
                    };

                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewData["ActiveForm"] = "SignIn";
                ViewData["ValidateMessage"] = e.Message;
                return View("Auth", utilisateur);
            }
        }
    }
}
