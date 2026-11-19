using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Web.Extensions;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Lernbereich: Karteikarten lernen und Lernpfade schrittweise bearbeiten.
    /// Für alle angemeldeten Benutzer – so können auch Lehrkräfte ihre Inhalte selbst ausprobieren.
    /// </summary>
    [Authorize]
    public class LernenController : Controller
    {
        private readonly IThemengebietService _themengebietService;
        private readonly IKarteikarteService _karteikarteService;
        private readonly IFrageService _frageService;
        private readonly IKarteikartenLernService _lernService;
        private readonly ILernpfadService _lernpfadService;

        public LernenController(
            IThemengebietService themengebietService,
            IKarteikarteService karteikarteService,
            IFrageService frageService,
            IKarteikartenLernService lernService,
            ILernpfadService lernpfadService)
        {
            _themengebietService = themengebietService;
            _karteikarteService = karteikarteService;
            _frageService = frageService;
            _lernService = lernService;
            _lernpfadService = lernpfadService;
        }

        // GET: /Lernen – Übersicht aller Themengebiete und Lernpfade
        public async Task<IActionResult> Index()
        {
            var benutzerId = User.GetBenutzerId();
            var themengebiete = await _themengebietService.GetAllAsync();
            var karten = await _karteikarteService.GetAllAsync();
            var fragen = await _frageService.GetAllAsync();

            var model = new LernenIndexViewModel
            {
                Lernpfade = await _lernpfadService.GetAllAsync()
            };

            foreach (var themengebiet in themengebiete)
            {
                var (gelernt, _) = await _lernService.ZaehleAsync(benutzerId, themengebiet.Id);
                model.Themengebiete.Add(new ThemengebietKachel
                {
                    Themengebiet = themengebiet,
                    AnzahlKarteikarten = karten.Count(k => k.ThemengebietId == themengebiet.Id),
                    AnzahlFragen = fragen.Count(f => f.ThemengebietId == themengebiet.Id),
                    GelernteKarteikarten = gelernt
                });
            }

            return View(model);
        }

        // GET: /Lernen/Karteikarten/1 – nächste Karte eines Themengebiets
        public async Task<IActionResult> Karteikarten(int id)
        {
            var themengebiet = await _themengebietService.GetByIdAsync(id);
            if (themengebiet is null)
            {
                return NotFound();
            }

            var benutzerId = User.GetBenutzerId();
            var (gelernt, gesamt) = await _lernService.ZaehleAsync(benutzerId, id);

            return View(new KarteikartenLernenViewModel
            {
                Themengebiet = themengebiet,
                Karte = await _lernService.NaechsteKarteAsync(benutzerId, id),
                Gelernt = gelernt,
                Gesamt = gesamt
            });
        }

        // POST: /Lernen/Bewerten – "Gewusst" / "Nicht gewusst"
        // Kommt die Karte aus einem Lernpfad, geht es danach mit dem nächsten Schritt weiter.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bewerten(int karteikarteId, bool gewusst, int themengebietId, int? lernpfadId, int? schritt)
        {
            await _lernService.BewertenAsync(User.GetBenutzerId(), karteikarteId, gewusst);

            if (lernpfadId.HasValue && schritt.HasValue)
            {
                return RedirectToAction(nameof(Lernpfad), new { id = lernpfadId, schritt = schritt + 1 });
            }

            return RedirectToAction(nameof(Karteikarten), new { id = themengebietId });
        }

        // POST: /Lernen/Zuruecksetzen – alle Karten eines Themengebiets nochmal lernen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Zuruecksetzen(int themengebietId)
        {
            await _lernService.ZuruecksetzenAsync(User.GetBenutzerId(), themengebietId);
            TempData["Erfolg"] = "Der Lernstand wurde zurückgesetzt. Viel Erfolg beim Wiederholen!";
            return RedirectToAction(nameof(Karteikarten), new { id = themengebietId });
        }

        // GET: /Lernen/Lernpfad/3?schritt=2 – ein Lernpfad Schritt für Schritt
        public async Task<IActionResult> Lernpfad(int id, int schritt = 1)
        {
            var lernpfad = await _lernpfadService.GetByIdAsync(id);
            if (lernpfad is null)
            {
                return NotFound();
            }

            var elemente = lernpfad.Elemente.OrderBy(e => e.Reihenfolge).ToList();
            if (elemente.Count == 0)
            {
                TempData["Fehler"] = "Dieser Lernpfad enthält noch keine Inhalte.";
                return RedirectToAction(nameof(Index));
            }

            // Hinter dem letzten Schritt: Abschlussseite anzeigen.
            if (schritt > elemente.Count)
            {
                ViewData["HatFragen"] = elemente.Any(e => e.Typ == ElementTyp.Frage);
                return View("LernpfadAbgeschlossen", lernpfad);
            }

            schritt = Math.Max(1, schritt);

            return View(new LernpfadSchrittViewModel
            {
                Lernpfad = lernpfad,
                Element = elemente[schritt - 1],
                Schritt = schritt,
                AnzahlSchritte = elemente.Count,
                HatFragen = elemente.Any(e => e.Typ == ElementTyp.Frage)
            });
        }
    }
}
