using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Core.Models;
using Pruefungstrainer.Web.Extensions;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    /// <summary>
    /// Quiz (zu einem Themengebiet oder Lernpfad) und Prüfungssimulation (alle Themen, mit Zeitlimit).
    /// </summary>
    [Authorize]
    public class QuizController : Controller
    {
        private const int AnzahlFragenQuiz = 10;
        private const int AnzahlFragenPruefung = 20;
        private const int MinutenPruefung = 30;

        private readonly IQuizService _quizService;
        private readonly IThemengebietService _themengebietService;
        private readonly ILernpfadService _lernpfadService;

        public QuizController(IQuizService quizService, IThemengebietService themengebietService, ILernpfadService lernpfadService)
        {
            _quizService = quizService;
            _themengebietService = themengebietService;
            _lernpfadService = lernpfadService;
        }

        // GET: /Quiz/Start?themengebietId=1  oder  /Quiz/Start?lernpfadId=3
        public async Task<IActionResult> Start(int? themengebietId, int? lernpfadId)
        {
            string titel;
            if (lernpfadId.HasValue)
            {
                var lernpfad = await _lernpfadService.GetByIdAsync(lernpfadId.Value);
                if (lernpfad is null)
                {
                    return NotFound();
                }

                titel = $"Abschlusstest: {lernpfad.Titel}";
                themengebietId = null;
            }
            else if (themengebietId.HasValue)
            {
                var themengebiet = await _themengebietService.GetByIdAsync(themengebietId.Value);
                if (themengebiet is null)
                {
                    return NotFound();
                }

                titel = $"Quiz: {themengebiet.Name}";
            }
            else
            {
                return BadRequest();
            }

            var fragen = await _quizService.ErstelleFragenAsync(themengebietId, lernpfadId, AnzahlFragenQuiz);
            return StarteSitzung(titel, fragen, lernpfadId, endetUmUtc: null);
        }

        // GET: /Quiz/Pruefung – Prüfungssimulation über alle Themengebiete mit Zeitlimit
        public async Task<IActionResult> Pruefung()
        {
            var fragen = await _quizService.ErstelleFragenAsync(null, null, AnzahlFragenPruefung);
            return StarteSitzung("Prüfungssimulation", fragen, null, DateTime.UtcNow.AddMinutes(MinutenPruefung));
        }

        // POST: /Quiz/Abgeben
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abgeben(List<QuizAntwortEingabe> antworten)
        {
            var sitzung = LeseSitzung();
            if (sitzung is null)
            {
                // Z.B. doppelt abgeschickt oder Session abgelaufen.
                TempData["Fehler"] = "Es läuft kein Quiz (mehr). Bitte starte ein neues.";
                return RedirectToAction("Index", "Lernen");
            }

            var ergebnisId = await _quizService.AuswertenAsync(
                User.GetBenutzerId(),
                sitzung.FrageIds,
                sitzung.LernpfadId,
                antworten.Select(a => new QuizAntwort
                {
                    FrageId = a.FrageId,
                    GewaehlteOptionIds = a.GewaehlteOptionIds,
                    Freitext = a.Freitext
                }));

            // Sitzung beenden, damit dasselbe Quiz nicht ein zweites Mal gewertet wird.
            HttpContext.Session.Remove(QuizSitzung.SessionSchluessel);

            return RedirectToAction(nameof(Ergebnis), new { id = ergebnisId });
        }

        // GET: /Quiz/Ergebnis/7
        public async Task<IActionResult> Ergebnis(int id)
        {
            var ergebnis = await _quizService.GetErgebnisAsync(id, User.GetBenutzerId());
            if (ergebnis is null)
            {
                return NotFound();
            }

            return View(ergebnis);
        }

        // POST: /Quiz/FreitextWerten – Selbsteinschätzung nach Vergleich mit der Musterlösung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FreitextWerten(int detailId, int ergebnisId)
        {
            if (!await _quizService.FreitextAlsRichtigWertenAsync(detailId, User.GetBenutzerId()))
            {
                TempData["Fehler"] = "Diese Antwort kann nicht (mehr) gewertet werden.";
            }

            return RedirectToAction(nameof(Ergebnis), new { id = ergebnisId });
        }

        // GET: /Quiz/Verlauf – alle eigenen Ergebnisse
        public async Task<IActionResult> Verlauf()
        {
            return View(await _quizService.GetErgebnisseAsync(User.GetBenutzerId()));
        }

        private IActionResult StarteSitzung(string titel, List<Frage> fragen, int? lernpfadId, DateTime? endetUmUtc)
        {
            if (fragen.Count == 0)
            {
                TempData["Fehler"] = "Hierzu gibt es noch keine Fragen.";
                return RedirectToAction("Index", "Lernen");
            }

            var sitzung = new QuizSitzung
            {
                Titel = titel,
                FrageIds = fragen.Select(f => f.Id).ToList(),
                LernpfadId = lernpfadId,
                EndetUmUtc = endetUmUtc
            };
            HttpContext.Session.SetString(QuizSitzung.SessionSchluessel, JsonSerializer.Serialize(sitzung));

            return View("Durchfuehren", new QuizDurchfuehrenViewModel
            {
                Titel = titel,
                Fragen = fragen,
                EndetUmUtc = endetUmUtc
            });
        }

        private QuizSitzung? LeseSitzung()
        {
            var json = HttpContext.Session.GetString(QuizSitzung.SessionSchluessel);
            return json is null ? null : JsonSerializer.Deserialize<QuizSitzung>(json);
        }
    }
}
