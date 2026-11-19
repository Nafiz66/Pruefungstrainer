using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Web.Models;

namespace Pruefungstrainer.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        // GET: / – Startseite; zeigt je nach Rolle passende Kacheln (siehe View)
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // Wird von app.UseExceptionHandler("/Home/Error") aufgerufen, wenn außerhalb
        // der Entwicklungsumgebung eine unbehandelte Exception auftritt.
        // Kein Caching, damit nie eine alte Fehlerseite ausgeliefert wird.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
