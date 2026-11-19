using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="IKarteikarteService"/> mit EF Core.
/// </summary>
public class KarteikarteService : IKarteikarteService
{
    private readonly ApplicationDbContext _context;

    public KarteikarteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Karteikarte>> GetAllAsync(int? themengebietId = null)
    {
        var abfrage = _context.Karteikarten
            .AsNoTracking()
            .Include(k => k.Themengebiet)
            .AsQueryable();

        if (themengebietId.HasValue)
        {
            abfrage = abfrage.Where(k => k.ThemengebietId == themengebietId.Value);
        }

        return await abfrage
            .OrderBy(k => k.Themengebiet.Name)
            .ThenBy(k => k.Id)
            .ToListAsync();
    }

    public async Task<Karteikarte?> GetByIdAsync(int id)
    {
        return await _context.Karteikarten
            .AsNoTracking()
            .Include(k => k.Themengebiet)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task CreateAsync(Karteikarte karteikarte)
    {
        karteikarte.Frage = karteikarte.Frage.Trim();
        karteikarte.Antwort = karteikarte.Antwort.Trim();

        _context.Karteikarten.Add(karteikarte);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Karteikarte karteikarte)
    {
        var vorhanden = await _context.Karteikarten.FindAsync(karteikarte.Id)
            ?? throw new KeyNotFoundException($"Karteikarte {karteikarte.Id} nicht gefunden.");

        vorhanden.Frage = karteikarte.Frage.Trim();
        vorhanden.Antwort = karteikarte.Antwort.Trim();
        vorhanden.ThemengebietId = karteikarte.ThemengebietId;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var karteikarte = await _context.Karteikarten.FindAsync(id);
        if (karteikarte is null)
        {
            return false;
        }

        // Restrict: Eine Karte, die in einem Lernpfad steckt, darf nicht verschwinden.
        if (await _context.LernpfadElemente.AnyAsync(e => e.KarteikarteId == id))
        {
            return false;
        }

        // Cascade: Der Lernfortschritt der Teilnehmer zu dieser Karte wird mitgelöscht.
        _context.Karteikarten.Remove(karteikarte);
        await _context.SaveChangesAsync();
        return true;
    }
}
