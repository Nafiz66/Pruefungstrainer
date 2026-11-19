using Microsoft.AspNetCore.Mvc.Rendering;
using Pruefungstrainer.Core.Interfaces;

namespace Pruefungstrainer.Web.Models;

/// <summary>
/// Erzeugt Einträge für Dropdown-Listen (&lt;select&gt;), die mehrere Controller brauchen.
/// </summary>
public static class Auswahllisten
{
    public static async Task<List<SelectListItem>> ThemengebieteAsync(IThemengebietService service)
    {
        var themengebiete = await service.GetAllAsync();
        return themengebiete
            .Select(t => new SelectListItem(t.Name, t.Id.ToString()))
            .ToList();
    }
}
