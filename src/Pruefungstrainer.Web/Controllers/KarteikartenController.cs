using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Verwaltung der Karteikarten – nur für Lehrkräfte.
    /// (Das Lernen mit Karteikarten passiert im LernenController.)
    /// </summary>
    [Authorize(Roles = nameof(UserRole.Lehrkraft))]
    public class KarteikartenController : Controller
    {
        private readonly IKarteikarteService _karteikarteService;
        private readonly IThemengebietService _themengebietService;

        public KarteikartenController(IKarteikarteService karteikarteService, IThemengebietService themengebietService)
        {
            _karteikarteService = karteikarteService;
            _themengebietService = themengebietService;
        }

        // GET: /Karteikarten?themengebietId=1
        public async Task<IActionResult> Index(int? themengebietId)
        {
            ViewData["ThemengebietAuswahl"] = await Auswahllisten.ThemengebieteAsync(_themengebietService);
            ViewData["ThemengebietId"] = themengebietId;
            return View(await _karteikarteService.GetAllAsync(themengebietId));
        }

        // GET: /Karteikarten/Create
        public async Task<IActionResult> Create(int? themengebietId)
        {
            var model = new KarteikarteFormViewModel
            {
                ThemengebietId = themengebietId ?? 0,
                ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService)
            };
            return View(model);
        }

        // POST: /Karteikarten/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KarteikarteFormViewModel model)
        {
            await PruefeThemengebietAsync(model.ThemengebietId);

            if (!ModelState.IsValid)
            {
                model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
                return View(model);
            }

            var karte = model.ZuEntitaet();
            karte.Id = 0;
            await _karteikarteService.CreateAsync(karte);

            TempData["Erfolg"] = "Die Karteikarte wurde angelegt.";
            return RedirectToAction(nameof(Index), new { themengebietId = model.ThemengebietId });
        }

        // GET: /Karteikarten/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var karte = await _karteikarteService.GetByIdAsync(id);
            if (karte is null)
            {
                return NotFound();
            }

            var model = KarteikarteFormViewModel.AusEntitaet(karte);
            model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
            return View(model);
        }

        // POST: /Karteikarten/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KarteikarteFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            await PruefeThemengebietAsync(model.ThemengebietId);

            if (!ModelState.IsValid)
            {
                model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
                return View(model);
            }

            try
            {
                await _karteikarteService.UpdateAsync(model.ZuEntitaet());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            TempData["Erfolg"] = "Die Karteikarte wurde gespeichert.";
            return RedirectToAction(nameof(Index), new { themengebietId = model.ThemengebietId });
        }

        // POST: /Karteikarten/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _karteikarteService.DeleteAsync(id))
            {
                TempData["Erfolg"] = "Die Karteikarte wurde gelöscht.";
            }
            else
            {
                TempData["Fehler"] = "Die Karteikarte kann nicht gelöscht werden, solange sie in einem Lernpfad verwendet wird.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PruefeThemengebietAsync(int themengebietId)
        {
            if (themengebietId > 0 && await _themengebietService.GetByIdAsync(themengebietId) is null)
            {
                ModelState.AddModelError(nameof(KarteikarteFormViewModel.ThemengebietId), "Das gewählte Themengebiet existiert nicht.");
            }
        }
    }
}
