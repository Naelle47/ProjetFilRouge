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
