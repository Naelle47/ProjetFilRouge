using Microsoft.AspNetCore.Mvc;
using ProjetFilRouge.Models;
using ProjetFilRouge.ViewModels;
using System.Linq;

namespace ProjetFilRouge.Controllers
{
    public class UtilisateursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UtilisateursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index()
        {
            var isAdmin = HttpContext.Session.GetString("UtilisateurAdmin");
            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "True")
            {
                var userId = HttpContext.Session.GetInt32("UtilisateurId");
                if (userId.HasValue)
                    return RedirectToAction("EditDelete", new { id = userId.Value });

                return RedirectToAction("Login", "Compte");
            }

            var utilisateurs = _context.Utilisateurs
                .Select(u => new UtilisateurViewModel
                {
                    UtilisateurId = u.UtilisateurId,
                    Nom = u.Nom,
                    Email = u.Email,
                    Admin = u.Admin
                }).ToList();

            return View(utilisateurs);
        }

        // GET: Edit + Delete combiné
        public IActionResult EditDelete(int id)
        {
            var isAdmin = HttpContext.Session.GetString("UtilisateurAdmin");
            var currentUserId = HttpContext.Session.GetInt32("UtilisateurId");

            if (string.IsNullOrEmpty(isAdmin)) return RedirectToAction("Login", "Compte");

            if (isAdmin != "True" && currentUserId != id)
            {
                // Non-admin ne peut modifier que son propre profil
                return RedirectToAction("EditDelete", new { id = currentUserId });
            }

            var utilisateur = _context.Utilisateurs.Find(id);
            if (utilisateur == null) return NotFound();

            var vm = new UtilisateurViewModel
            {
                UtilisateurId = utilisateur.UtilisateurId,
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                Admin = utilisateur.Admin
            };

            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDelete(UtilisateurViewModel vm)
        {
            var isAdmin = HttpContext.Session.GetString("UtilisateurAdmin");
            var currentUserId = HttpContext.Session.GetInt32("UtilisateurId");

            if (string.IsNullOrEmpty(isAdmin)) return RedirectToAction("Login", "Compte");

            if (isAdmin != "True" && currentUserId != vm.UtilisateurId)
            {
                return RedirectToAction("EditDelete", new { id = currentUserId });
            }

            if (ModelState.IsValid)
            {
                var utilisateur = _context.Utilisateurs.Find(vm.UtilisateurId);
                if (utilisateur == null) return NotFound();

                utilisateur.Nom = vm.Nom;
                utilisateur.Email = vm.Email;
                if (isAdmin == "True") utilisateur.Admin = vm.Admin;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int UtilisateurId)
        {
            var isAdmin = HttpContext.Session.GetString("UtilisateurAdmin");
            var currentUserId = HttpContext.Session.GetInt32("UtilisateurId");

            if (string.IsNullOrEmpty(isAdmin)) return RedirectToAction("Login", "Compte");

            if (isAdmin != "True" && currentUserId != UtilisateurId)
            {
                return RedirectToAction("EditDelete", new { id = currentUserId });
            }

            var utilisateur = _context.Utilisateurs.Find(UtilisateurId);
            if (utilisateur == null) return NotFound();

            _context.Utilisateurs.Remove(utilisateur);
            _context.SaveChanges();

            if (isAdmin == "True")
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction("Login", "Compte");
        }

        // GET: Détails
        public IActionResult Details(int id)
        {
            var isAdmin = HttpContext.Session.GetString("UtilisateurAdmin");
            var currentUserId = HttpContext.Session.GetInt32("UtilisateurId");

            if (string.IsNullOrEmpty(isAdmin)) return RedirectToAction("Login", "Compte");

            if (isAdmin != "True" && currentUserId != id)
            {
                return RedirectToAction("Details", new { id = currentUserId });
            }

            var utilisateur = _context.Utilisateurs.Find(id);
            if (utilisateur == null) return NotFound();

            var vm = new UtilisateurViewModel
            {
                UtilisateurId = utilisateur.UtilisateurId,
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                Admin = utilisateur.Admin
            };

            return View(vm);
        }
    }
}
