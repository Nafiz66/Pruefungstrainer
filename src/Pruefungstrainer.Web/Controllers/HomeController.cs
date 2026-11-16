using Microsoft.AspNetCore.Mvc;

namespace Pruefungstrainer.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: /
        public IActionResult Index()
        {
            // Leitet die Startseite vorerst auf den Fragen-Controller weiter 
            // oder zeigt eine Begrüßung an.
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}