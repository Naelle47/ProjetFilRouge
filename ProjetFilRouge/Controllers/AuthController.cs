using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Web;
using AspNetCoreGeneratedDocument;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Npgsql;
using ProjetFilRouge.Models;
using BC = BCrypt.Net.BCrypt;
using ProjetFilRouge.ViewModel;

namespace ProjetFilRouge.Controllers
{
    public class AuthController : Controller
    {
        private readonly string _connexionString;

        public AuthController(IConfiguration configuration)
        {
            _connexionString = configuration.GetConnectionString("GestionCatalogue")!;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]

        public IActionResult SignUp([FromForm] AuthViewModel.SignUpViewModel model, [FromForm] string? returnUrl = null)
        {
            if (ModelState.IsValid) 
            {
                ViewData["ActiveForm"] = "SignUp";
                ViewData["ReturnUrl"] = returnUrl;
                return View("Auth");
            }

            try
            {
                using var connexion = new NpgsqlConnection(_connexionString);
            }
            catch (Exception ex) 
            { 

            }
            














































        }





















} // end of namespace
