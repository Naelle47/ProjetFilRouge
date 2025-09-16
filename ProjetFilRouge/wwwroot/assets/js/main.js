// PRIMARY NAVIGATION SCRIPT
const primaryHeader = document.querySelector(".primary-header");
const navToggle = document.querySelector(".mobile-nav-toggle");
const primaryNav = document.querySelector(".primary-navigation");
navToggle.addEventListener("click", () => {
  primaryNav.hasAttribute("data-visible")
    ? navToggle.setAttribute("aria-expanded", false)
    : navToggle.setAttribute("aria-expanded", true);
  primaryNav.toggleAttribute("data-visible");
  primaryHeader.toggleAttribute("data-overlay");
});

// PRIVACY POLICY
document.querySelectorAll('.accordion-header').forEach(header => {
    header.addEventListener('click', () => {
      const item = header.parentElement;
      item.classList.toggle('active');
    });
});

// PASSWORD VALIDATION
const password = document.getElementById("signup-password");
const confirmPassword = document.getElementById("signup-confirm-password");
const message = document.getElementById("password-match-message");

document.querySelector("form").addEventListener("submit", function (e) {
    if (password.value !== confirmPassword.value) {
        e.preventDefault();
        message.textContent = "Passwords do not match!";
    } else {
        message.textContent = "";
    }
});

// API FETCH
function rechercheJeu() {
    const recherche = document.getElementById("recherche").value;
    const reponse = await fetch("http://localhost:5106/Jeux/RechercheJeu?titre=" + recherche);
    const jeux = await reponse.json();
    document.getElementById("affichageRecherche").innerHTML = "";
    jeux.forEach(j => AfficherJeu(j));
}
function AfficherJeu(jeu) {
    let j = document.createElement("div");
    let titre = document.createElement("a");
    titre.href = "http://localhost:5106/Jeux/Detail/" + jeu.id;
    titre.textContent = jeu.titre;
    j.appendChild(titre);
    document.getElementById("affichageRecherche").appendChild(j);
}
let barreRecherche = document.getElementById("recherche");
barreRecherche.addEventListener("input", rechercheJeu);
async function toggleRecherche() {
    let divRecherche = document.getElementById("affichageRecherche");
    if (divRecherche.classList.contains("actif")) {
        await delay(500);
    }
    divRecherche.classList.toggle("actif");
}
barreRecherche.addEventListener("focus", toggleRecherche);
barreRecherche.addEventListener("focusout", toggleRecherche);