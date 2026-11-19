using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="IThemengebietService"/> mit EF Core.
/// </summary>
public class ThemengebietService : IThemengebietService
{
    private readonly ApplicationDbContext _context;

    // Der DbContext wird per Dependency Injection übergeben (Konstruktor-Injektion).
    public ThemengebietService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Themengebiet>> GetAllAsync()
    {
        // AsNoTracking: Wir lesen nur, deshalb muss EF Core die Objekte nicht überwachen. Das ist schneller.
        return await _context.Themengebiete
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Themengebiet?> GetByIdAsync(int id)
    {
        return await _context.Themengebiete
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name, int? ausnahmeId = null)
    {
        var bereinigt = name.Trim();

        return await _context.Themengebiete
            .AnyAsync(t => t.Name == bereinigt && t.Id != ausnahmeId);
    }

    public async Task CreateAsync(Themengebiet themengebiet)
    {
        themengebiet.Name = themengebiet.Name.Trim();

        _context.Themengebiete.Add(themengebiet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Themengebiet themengebiet)
    {
        // Den gespeicherten Datensatz laden und nur die erlaubten Felder überschreiben.
        var vorhanden = await _context.Themengebiete.FindAsync(themengebiet.Id)
            ?? throw new KeyNotFoundException($"Themengebiet {themengebiet.Id} nicht gefunden.");

        vorhanden.Name = themengebiet.Name.Trim();
        vorhanden.Beschreibung = themengebiet.Beschreibung;
        vorhanden.Kategorie = themengebiet.Kategorie;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var themengebiet = await _context.Themengebiete.FindAsync(id);
        if (themengebiet is null)
        {
            return false;
        }

        // DeleteBehavior.Restrict: Hängen noch Daten daran, würde die Datenbank das Löschen verweigern.
        // Wir prüfen es vorher selbst, damit wir eine klare Meldung statt einer Exception bekommen.
        var wirdVerwendet =
            await _context.Fragen.AnyAsync(f => f.ThemengebietId == id) ||
            await _context.Karteikarten.AnyAsync(k => k.ThemengebietId == id) ||
            await _context.Lernpfade.AnyAsync(l => l.ThemengebietId == id);

        if (wirdVerwendet)
        {
            return false;
        }

        _context.Themengebiete.Remove(themengebiet);
        await _context.SaveChangesAsync();
        return true;
    }
}
