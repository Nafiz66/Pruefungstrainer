using Microsoft.AspNetCore.Mvc;
using Pruefungstrainer.Core.Interfaces;

namespace Pruefungstrainer.Web.Controllers
{
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
    }
}
