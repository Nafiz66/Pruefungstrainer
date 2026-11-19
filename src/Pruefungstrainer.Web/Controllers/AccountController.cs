using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Anmelden, Abmelden und Selbstregistrierung (als Teilnehmer).
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IBenutzerService _benutzerService;

        public AccountController(IBenutzerService benutzerService)
        {
            _benutzerService = benutzerService;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var benutzer = await _benutzerService.AnmeldenAsync(model.Email, model.Passwort);
            if (benutzer is null)
            {
                // Bewusst allgemein: nicht verraten, ob die E-Mail existiert oder nur das Passwort falsch ist.
                ModelState.AddModelError(string.Empty, "E-Mail oder Passwort ist falsch.");
                return View(model);
            }

            await AnmeldenAsync(benutzer);

            // Nur auf Seiten der eigenen Anwendung zurückleiten (Schutz vor "Open Redirect").
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Registrieren
        [AllowAnonymous]
        public IActionResult Registrieren()
        {
            return View(new BenutzerFormViewModel());
        }

        // POST: /Account/Registrieren
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrieren(BenutzerFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Email) && await _benutzerService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Diese E-Mail-Adresse ist bereits registriert.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Selbstregistrierung ergibt immer die Rolle Teilnehmer – egal, was das Formular schickt.
            // Lehrkräfte werden nur von einer Lehrkraft in der Benutzerverwaltung angelegt.
            var benutzer = await _benutzerService.RegistrierenAsync(
                model.Vorname, model.Nachname, model.Email, model.Passwort, UserRole.Teilnehmer);

            await AnmeldenAsync(benutzer);
            TempData["Erfolg"] = $"Willkommen, {benutzer.Vorname}! Dein Konto wurde angelegt.";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout – POST statt GET, damit ein fremder Link niemanden abmelden kann.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ZugriffVerweigert – angemeldet, aber falsche Rolle
        [AllowAnonymous]
        public IActionResult ZugriffVerweigert()
        {
            return View();
        }

        /// <summary>
        /// Erstellt das Login-Cookie. Die Claims sind "Ausweisdaten", die bei jeder Anfrage mitkommen:
        /// Id, Name, E-Mail und Rolle. [Authorize(Roles = "...")] prüft den Rollen-Claim.
        /// </summary>
        private async Task AnmeldenAsync(ApplicationUser benutzer)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, benutzer.Id.ToString()),
                new(ClaimTypes.Name, $"{benutzer.Vorname} {benutzer.Nachname}"),
                new(ClaimTypes.Email, benutzer.Email),
                new(ClaimTypes.Role, benutzer.Rolle.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
