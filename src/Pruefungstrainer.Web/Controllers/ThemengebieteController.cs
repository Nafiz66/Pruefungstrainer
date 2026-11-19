using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Verwaltung der Themengebiete – nur für Lehrkräfte.
    /// </summary>
    [Authorize(Roles = nameof(UserRole.Lehrkraft))]
    public class ThemengebieteController : Controller
    {
        private readonly IThemengebietService _service;

        // ASP.NET Core sieht "IThemengebietService" und übergibt automatisch einen
        // ThemengebietService, so wie wir es in Program.cs registriert haben.
        public ThemengebieteController(IThemengebietService service)
        {
            _service = service;
        }

        // GET: /Themengebiete
        public async Task<IActionResult> Index()
        {
            var themengebiete = await _service.GetAllAsync();
            return View(themengebiete);
        }

        // GET: /Themengebiete/Create – zeigt das leere Formular
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Themengebiete/Create – nimmt das abgeschickte Formular entgegen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Beschreibung,Kategorie")] Themengebiet themengebiet)
        {
            // Eigene Prüfung zusätzlich zu den [Required]/[MaxLength]-Attributen der Entität
            if (!string.IsNullOrWhiteSpace(themengebiet.Name) &&
                await _service.NameExistsAsync(themengebiet.Name))
            {
                ModelState.AddModelError(nameof(Themengebiet.Name), "Ein Themengebiet mit diesem Namen existiert bereits.");
            }

            if (!ModelState.IsValid)
            {
                // Fehler: dasselbe Formular nochmal zeigen, mit den eingegebenen Werten und den Fehlermeldungen
                return View(themengebiet);
            }

            await _service.CreateAsync(themengebiet);
            TempData["Erfolg"] = $"Themengebiet \"{themengebiet.Name}\" wurde angelegt.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Themengebiete/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var themengebiet = await _service.GetByIdAsync(id);
            if (themengebiet is null)
            {
                return NotFound();
            }

            return View(themengebiet);
        }

        // POST: /Themengebiete/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Beschreibung,Kategorie")] Themengebiet themengebiet)
        {
            // Die Id aus der URL muss zur Id im Formular passen – sonst wurde manipuliert.
            if (id != themengebiet.Id)
            {
                return BadRequest();
            }

            // Beim Bearbeiten das eigene Themengebiet von der Namensprüfung ausnehmen.
            if (!string.IsNullOrWhiteSpace(themengebiet.Name) &&
                await _service.NameExistsAsync(themengebiet.Name, ausnahmeId: id))
            {
                ModelState.AddModelError(nameof(Themengebiet.Name), "Ein Themengebiet mit diesem Namen existiert bereits.");
            }

            if (!ModelState.IsValid)
            {
                return View(themengebiet);
            }

            try
            {
                await _service.UpdateAsync(themengebiet);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            TempData["Erfolg"] = $"Themengebiet \"{themengebiet.Name}\" wurde gespeichert.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Themengebiete/Delete/5
        // Die Sicherheitsabfrage passiert im Browser (confirm), gelöscht wird nur per POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _service.DeleteAsync(id))
            {
                TempData["Erfolg"] = "Das Themengebiet wurde gelöscht.";
            }
            else
            {
                TempData["Fehler"] = "Das Themengebiet kann nicht gelöscht werden, solange noch Fragen, Karteikarten oder Lernpfade darin enthalten sind.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
