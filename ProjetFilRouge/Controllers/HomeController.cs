using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjetFilRouge.Models;

namespace ProjetFilRouge.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            Response.StatusCode = code ?? 500; // Définit le code HTTP de la réponse
            return View("Error", code);
        }

        // Formulaire de contact
        [HttpGet]
        public IActionResult Contact()
        { 
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(Contact model) 
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            // Traitement ici (ex. : envoi d'email, sauvegarde, log)
            ViewData["ValidateMessage"] = "Merci pour votre message ! Nous reviendrons vers vous rapidement.";
            return RedirectToAction("Contact");
        }

    }
}
