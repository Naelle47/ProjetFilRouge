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
                    UriBuilder uriBuilder = new UriBuilder
                    {
                        Port = 5103,
                        Path = "/Access/VerifyEmail",
                        Query = $"email={HttpUtility.UrlEncode(utilisateur.Email)}&token={HttpUtility.UrlEncode(token)}"
                    };

                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress("app@nivo.fr"),
                        Subject = "Vérification d'email",
                        Body = $"<a href=\"{uriBuilder.Uri}\">Vérifier l'email</a>",
                        IsBodyHtml = true
                    };
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
        [HttpGet]
        public IActionResult VerifyEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                ViewData["ActiveForm"] = "SignIn";
                ViewData["ValidateMessage"] = "Lien de vérification invalide.";
                return View("Authentification");
            }

            string query = @"UPDATE utilisateurs 
                             SET emailverified = true, verificationtoken = null 
                             WHERE email = @Email AND verificationtoken = @Token";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int rows = connexion.Execute(query, new { Email = email, Token = token });

                    if (rows == 0)
                    {
                        ViewData["ActiveForm"] = "SignIn";
                        ViewData["ValidateMessage"] = "Lien de vérification invalide ou déjà utilisé.";
                        return View("Authentification");
                    }

                    ViewData["ActiveForm"] = "SignIn";
                    ViewData["ValidateMessage"] = "Email vérifié avec succès, vous pouvez maintenant vous connecter.";
                    return View("Authentification");
                }
            }
            catch (Exception e)
            {
                ViewData["ActiveForm"] = "SignIn";
                ViewData["ValidateMessage"] = e.Message;
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

            string query = @"SELECT * FROM utilisateurs WHERE email = @Email";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    var user = connexion.QueryFirstOrDefault<Utilisateur>(query, new { utilisateur.Email });

                    if (user == null || !BCrypt.Net.BCrypt.Verify(utilisateur.Password, user.Password))
                        throw new Exception("Email ou mot de passe incorrect.");

                    if (!user.EmailVerified)
                        throw new Exception("Veuillez vérifier votre adresse e-mail avant de vous connecter.");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
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
