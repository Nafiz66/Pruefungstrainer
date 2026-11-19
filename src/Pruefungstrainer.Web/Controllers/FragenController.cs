using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Verwaltung der Prüfungsfragen – nur für Lehrkräfte.
    /// Greift nicht mehr direkt auf den DbContext zu, sondern über IFrageService.
    /// </summary>
    [Authorize(Roles = nameof(UserRole.Lehrkraft))]
    public class FragenController : Controller
    {
        private readonly IFrageService _frageService;
        private readonly IThemengebietService _themengebietService;

        public FragenController(IFrageService frageService, IThemengebietService themengebietService)
        {
            _frageService = frageService;
            _themengebietService = themengebietService;
        }

        // GET: /Fragen?themengebietId=1 – optional nach Themengebiet gefiltert
        public async Task<IActionResult> Index(int? themengebietId)
        {
            ViewData["ThemengebietAuswahl"] = await Auswahllisten.ThemengebieteAsync(_themengebietService);
            ViewData["ThemengebietId"] = themengebietId;
            return View(await _frageService.GetAllAsync(themengebietId));
        }

        // GET: /Fragen/Create
        public async Task<IActionResult> Create(int? themengebietId)
        {
            var model = new FrageFormViewModel { ThemengebietId = themengebietId ?? 0 };
            model.FuelleAntwortfelderAuf();
            model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
            return View(model);
        }

        // POST: /Fragen/Create
        // Kein [Bind] nötig: Das ViewModel enthält nur Felder, die das Formular ändern darf.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FrageFormViewModel model)
        {
            await PruefeThemengebietAsync(model.ThemengebietId);

            if (!ModelState.IsValid)
            {
                return await FormularErneutAnzeigenAsync(model);
            }

            var frage = model.ZuEntitaet();
            frage.Id = 0; // Neue Frage: Die Id vergibt die Datenbank.
            await _frageService.CreateAsync(frage);

            TempData["Erfolg"] = "Die Frage wurde angelegt.";
            return RedirectToAction(nameof(Index), new { themengebietId = model.ThemengebietId });
        }

        // GET: /Fragen/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var frage = await _frageService.GetByIdAsync(id);
            if (frage is null)
            {
                return NotFound();
            }

            var model = FrageFormViewModel.AusEntitaet(frage);
            model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
            return View(model);
        }

        // POST: /Fragen/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FrageFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            await PruefeThemengebietAsync(model.ThemengebietId);

            if (!ModelState.IsValid)
            {
                return await FormularErneutAnzeigenAsync(model);
            }

            try
            {
                await _frageService.UpdateAsync(model.ZuEntitaet());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            TempData["Erfolg"] = "Die Frage wurde gespeichert.";
            return RedirectToAction(nameof(Index), new { themengebietId = model.ThemengebietId });
        }

        // POST: /Fragen/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _frageService.DeleteAsync(id))
            {
                TempData["Erfolg"] = "Die Frage wurde gelöscht.";
            }
            else
            {
                TempData["Fehler"] = "Die Frage kann nicht gelöscht werden, weil sie in einem Lernpfad oder in gespeicherten Prüfungsergebnissen verwendet wird.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Die Id kommt aus dem Formular und könnte manipuliert sein – also gegen die Datenbank prüfen.
        private async Task PruefeThemengebietAsync(int themengebietId)
        {
            if (themengebietId > 0 && await _themengebietService.GetByIdAsync(themengebietId) is null)
            {
                ModelState.AddModelError(nameof(FrageFormViewModel.ThemengebietId), "Das gewählte Themengebiet existiert nicht.");
            }
        }

        private async Task<IActionResult> FormularErneutAnzeigenAsync(FrageFormViewModel model)
        {
            model.FuelleAntwortfelderAuf();
            model.ThemengebietAuswahl = await Auswahllisten.ThemengebieteAsync(_themengebietService);
            return View(model);
        }
    }
}
