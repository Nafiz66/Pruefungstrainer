using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Web.Extensions;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Benutzerverwaltung – nur für Lehrkräfte.
    /// </summary>
    [Authorize(Roles = nameof(UserRole.Lehrkraft))]
    public class BenutzerController : Controller
    {
        private readonly IBenutzerService _benutzerService;

        public BenutzerController(IBenutzerService benutzerService)
        {
            _benutzerService = benutzerService;
        }

        // GET: /Benutzer
        public async Task<IActionResult> Index()
        {
            return View(await _benutzerService.GetAllAsync());
        }

        // GET: /Benutzer/Create
        public IActionResult Create()
        {
            return View(new BenutzerFormViewModel());
        }

        // POST: /Benutzer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenutzerFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Email) && await _benutzerService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Diese E-Mail-Adresse ist bereits registriert.");
            }

            if (!Enum.IsDefined(model.Rolle))
            {
                ModelState.AddModelError(nameof(model.Rolle), "Ungültige Rolle.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _benutzerService.RegistrierenAsync(model.Vorname, model.Nachname, model.Email, model.Passwort, model.Rolle);

            TempData["Erfolg"] = $"Benutzer \"{model.Vorname} {model.Nachname}\" wurde als {model.Rolle} angelegt.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Benutzer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Sich selbst zu löschen würde die eigene Sitzung ungültig machen – und evtl. die letzte Lehrkraft entfernen.
            if (id == User.GetBenutzerId())
            {
                TempData["Fehler"] = "Du kannst dich nicht selbst löschen.";
                return RedirectToAction(nameof(Index));
            }

            if (await _benutzerService.DeleteAsync(id))
            {
                TempData["Erfolg"] = "Der Benutzer wurde gelöscht.";
            }
            else
            {
                TempData["Fehler"] = "Der Benutzer wurde nicht gefunden.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
