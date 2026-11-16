using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Web.Controllers
{
    public class FragenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FragenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Fragen/
        public async Task<IActionResult> Index()
        {
            // Holt alle Fragen aus der SQLite-Datenbank
            var fragenList = await _context.Fragen.ToListAsync();
            return View(fragenList);
        }
    }
}