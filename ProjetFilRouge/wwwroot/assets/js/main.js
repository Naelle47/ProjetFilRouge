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
//const password = document.getElementById("signup-password");
//const confirmPassword = document.getElementById("signup-confirm-password");
//const message = document.getElementById("password-match-message");

//document.querySelector("form").addEventListener("submit", function (e) {
//    if (password.value !== confirmPassword.value) {
//        e.preventDefault();
//        message.textContent = "Passwords do not match!";
//    } else {
//        message.textContent = "";
//    }
//});

// API FETCH
async function rechercheJeu() {
    const recherche = document.getElementById("recherche").value;

    console.log("Recherche envoyée:", recherche); // log input

    const reponse = await fetch("http://localhost:5106/Jeux/RechercheJeu?titre=" + recherche);

    console.log("Statut de la réponse:", reponse.status); // log status

    const jeux = await reponse.json();

    console.log("Jeux reçus:", jeux); // log full JSON array

    document.getElementById("affichageRecherche").innerHTML = ""; // 
    jeux.forEach(j => AfficherJeu(j));
}
function AfficherJeu(jeu) {

    console.log("Affichage d’un jeu:", jeu); // log each object
    
    let j = document.createElement("div");
    let titre = document.createElement("a");
    titre.href = "http://localhost:5106/Jeux/Detail/" + jeu.jeuId;
    titre.textContent = jeu.titre;
    j.appendChild(titre);
    document.getElementById("affichageRecherche").appendChild(j);
}

let barreRecherche = document.getElementById("recherche");
barreRecherche.addEventListener("input", rechercheJeu);

async function toggleRecherche() {
    let divRecherche = document.getElementById("affichageRecherche");
    if (divRecherche.classList.contains("active")) {
        await delay(500);
    }
    divRecherche.classList.toggle("active");
}
barreRecherche.addEventListener("focus", toggleRecherche);
barreRecherche.addEventListener("focusout", toggleRecherche);