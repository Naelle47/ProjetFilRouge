using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjetFilRouge.Models
{
    public class EditeurJeuxViewModel
    {
        // Champs pour le formulaire d'ajout
        [Required(ErrorMessage = "Le titre est obligatoire.")]
        public required string Titre { get; set; }

        [Required(ErrorMessage = "Le temps de jeu est obligatoire.")]
        [Display(Name = "Temps de jeu (min)")]
        public int TempsJeuMoyen { get; set; }

        [Required(ErrorMessage = "Le nombre de joueurs est obligatoire.")]
        [Display(Name = "Nombre de joueurs")]
        public int NombreJoueursRecommandes { get; set; }

        [Required(ErrorMessage = "La catégorie est obligatoire.")]
        public required string Categorie { get; set; }

        [Required(ErrorMessage = "La date de sortie est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de sortie")]
        public DateTime DateSortie { get; set; }

        // Liste des jeux pour la galerie
        public List<Jeu> Jeux { get; set; } = new List<Jeu>();
    }
}
