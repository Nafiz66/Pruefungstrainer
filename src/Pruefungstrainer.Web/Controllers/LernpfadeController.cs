using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Verwaltung der Lernpfade und ihrer Elemente – nur für Lehrkräfte.
    /// (Das Bearbeiten eines Lernpfads als Teilnehmer passiert im LernenController.)
    /// </summary>
    [Authorize(Roles = nameof(UserRole.Lehrkraft))]
    public class LernpfadeController : Controller
    {
        private readonly ILernpfadService _lernpfadService;
        private readonly IThemengebietService _themengebietService;
        private readonly IKarteikarteService _karteikarteService;
        private readonly IFrageService _frageService;

        public LernpfadeController(
            ILernpfadService lernpfadService,
            IThemengebietService themengebietService,
            IKarteikarteService karteikarteService,
            IFrageService frageService)
        {
            _lernpfadService = lernpfadService;
            _themengebietService = themengebietService;
            _karteikarteService = karteikarteService;
            _frageService = frageService;
        }

        // GET: /Lernpfade
        public async Task<IActionResult> Index()
        {
            return View(await _lernpfadService.GetAllAsync());
        }

        // GET: /Lernpfade/Create
        public async Task<IActionResult> Create()
        {
            var model = new LernpfadFormViewModel
            {
                ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService)
            };
            return View(model);
        }

        // POST: /Lernpfade/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LernpfadFormViewModel model)
        {
            await PruefeThemengebietAsync(model.ThemengebietId);

            if (!ModelState.IsValid)
            {
                model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
                return View(model);
            }

            var lernpfad = model.ZuEntitaet();
            lernpfad.Id = 0;
            await _lernpfadService.CreateAsync(lernpfad);

            // Direkt weiter zu den Elementen – ein leerer Lernpfad ist noch nicht sinnvoll.
            TempData["Erfolg"] = "Der Lernpfad wurde angelegt. Füge jetzt Karteikarten und Fragen hinzu.";
            return RedirectToAction(nameof(Elemente), new { id = lernpfad.Id });
        }

        // GET: /Lernpfade/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var lernpfad = await _lernpfadService.GetByIdAsync(id);
            if (lernpfad is null)
            {
                return NotFound();
            }

            var model = LernpfadFormViewModel.AusEntitaet(lernpfad);
            model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
            return View(model);
        }

        // POST: /Lernpfade/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LernpfadFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var vorhanden = await _lernpfadService.GetByIdAsync(id);
            if (vorhanden is null)
            {
                return NotFound();
            }

            // Die Elemente gehören zum bisherigen Themengebiet – ein Wechsel würde den Lernpfad inkonsistent machen.
            if (vorhanden.Elemente.Count > 0 && vorhanden.ThemengebietId != model.ThemengebietId)
            {
                ModelState.AddModelError(nameof(model.ThemengebietId),
                    "Das Themengebiet kann nicht geändert werden, solange der Lernpfad Elemente enthält.");
            }

            await PruefeThemengebietAsync(model.ThemengebietId);

            if (!ModelState.IsValid)
            {
                model.HatElemente = vorhanden.Elemente.Count > 0;
                model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
                return View(model);
            }

            await _lernpfadService.UpdateAsync(model.ZuEntitaet());

            TempData["Erfolg"] = "Der Lernpfad wurde gespeichert.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Lernpfade/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _lernpfadService.DeleteAsync(id))
            {
                TempData["Erfolg"] = "Der Lernpfad wurde gelöscht. Bereits erzielte Prüfungsergebnisse bleiben erhalten.";
            }
            else
            {
                TempData["Fehler"] = "Der Lernpfad wurde nicht gefunden.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Lernpfade/Elemente/5 – Inhalte des Lernpfads zusammenstellen
        public async Task<IActionResult> Elemente(int id)
        {
            var lernpfad = await _lernpfadService.GetByIdAsync(id);
            if (lernpfad is null)
            {
                return NotFound();
            }

            // Zur Auswahl stehen nur Inhalte desselben Themengebiets, die noch nicht im Lernpfad sind.
            var enthalteneKarten = lernpfad.Elemente.Where(e => e.KarteikarteId.HasValue).Select(e => e.KarteikarteId!.Value).ToHashSet();
            var enthalteneFragen = lernpfad.Elemente.Where(e => e.FrageId.HasValue).Select(e => e.FrageId!.Value).ToHashSet();

            var karten = await _karteikarteService.GetAllAsync(lernpfad.ThemengebietId);
            var fragen = await _frageService.GetAllAsync(lernpfad.ThemengebietId);

            var model = new LernpfadElementeViewModel
            {
                Lernpfad = lernpfad,
                VerfuegbareKarteikarten = karten
                    .Where(k => !enthalteneKarten.Contains(k.Id))
                    .Select(k => new SelectListItem(Kuerzen(k.Frage), k.Id.ToString()))
                    .ToList(),
                VerfuegbareFragen = fragen
                    .Where(f => !enthalteneFragen.Contains(f.Id))
                    .Select(f => new SelectListItem(Kuerzen(f.Fragetext), f.Id.ToString()))
                    .ToList()
            };

            return View(model);
        }

        // POST: /Lernpfade/ElementHinzufuegen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ElementHinzufuegen(int lernpfadId, ElementTyp typ, int inhaltId)
        {
            var fehler = await _lernpfadService.ElementHinzufuegenAsync(lernpfadId, typ, inhaltId);
            if (fehler is null)
            {
                TempData["Erfolg"] = "Das Element wurde hinzugefügt.";
            }
            else
            {
                TempData["Fehler"] = fehler;
            }

            return RedirectToAction(nameof(Elemente), new { id = lernpfadId });
        }

        // POST: /Lernpfade/ElementEntfernen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ElementEntfernen(int lernpfadId, int elementId)
        {
            await _lernpfadService.ElementEntfernenAsync(elementId);
            return RedirectToAction(nameof(Elemente), new { id = lernpfadId });
        }

        // POST: /Lernpfade/ElementVerschieben
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ElementVerschieben(int lernpfadId, int elementId, bool nachOben)
        {
            await _lernpfadService.ElementVerschiebenAsync(elementId, nachOben);
            return RedirectToAction(nameof(Elemente), new { id = lernpfadId });
        }

        private async Task PruefeThemengebietAsync(int themengebietId)
        {
            if (themengebietId > 0 && await _themengebietService.GetByIdAsync(themengebietId) is null)
            {
                ModelState.AddModelError(nameof(LernpfadFormViewModel.ThemengebietId), "Das gewählte Themengebiet existiert nicht.");
            }
        }

        private static string Kuerzen(string text) => text.Length <= 80 ? text : text[..77] + "...";
    }
}
