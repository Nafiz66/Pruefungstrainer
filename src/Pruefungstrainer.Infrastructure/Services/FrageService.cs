using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="IFrageService"/> mit EF Core.
/// </summary>
public class FrageService : IFrageService
{
    private readonly ApplicationDbContext _context;

    public FrageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Frage>> GetAllAsync(int? themengebietId = null)
    {
        var abfrage = _context.Fragen
            .AsNoTracking()
            .Include(f => f.Themengebiet)
            .AsQueryable();

        if (themengebietId.HasValue)
        {
            abfrage = abfrage.Where(f => f.ThemengebietId == themengebietId.Value);
        }

        return await abfrage
            .OrderBy(f => f.Themengebiet.Name)
            .ThenBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<Frage?> GetByIdAsync(int id)
    {
        return await _context.Fragen
            .AsNoTracking()
            .Include(f => f.Themengebiet)
            .Include(f => f.Antwortoptionen)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task CreateAsync(Frage frage)
    {
        BereinigeNachTyp(frage);

        // EF Core speichert die Antwortoptionen aus frage.Antwortoptionen automatisch mit.
        _context.Fragen.Add(frage);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Frage frage)
    {
        var vorhanden = await _context.Fragen
            .Include(f => f.Antwortoptionen)
            .FirstOrDefaultAsync(f => f.Id == frage.Id)
            ?? throw new KeyNotFoundException($"Frage {frage.Id} nicht gefunden.");

        vorhanden.Fragetext = frage.Fragetext;
        vorhanden.Typ = frage.Typ;
        vorhanden.Punkte = frage.Punkte;
        vorhanden.MusterloesungFreitext = frage.MusterloesungFreitext;
        vorhanden.ThemengebietId = frage.ThemengebietId;

        // Antwortoptionen komplett ersetzen: einfacher und weniger fehleranfällig als
        // einzelne Optionen abzugleichen. Prüfungsergebnisse speichern die gegebene
        // Antwort als Text und verweisen nicht auf Antwortoptionen – das ist also gefahrlos.
        _context.Antwortoptionen.RemoveRange(vorhanden.Antwortoptionen);
        vorhanden.Antwortoptionen = frage.Antwortoptionen
            .Select(a => new Antwortoption { Text = a.Text, IstKorrekt = a.IstKorrekt })
            .ToList();

        BereinigeNachTyp(vorhanden);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var frage = await _context.Fragen.FindAsync(id);
        if (frage is null)
        {
            return false;
        }

        // Restrict: Fragen in Lernpfaden oder bereits beantwortete Fragen bleiben erhalten,
        // damit Lernpfade und Prüfungsergebnisse nachvollziehbar bleiben.
        var wirdVerwendet =
            await _context.LernpfadElemente.AnyAsync(e => e.FrageId == id) ||
            await _context.PruefungsergebnisFragen.AnyAsync(d => d.FrageId == id);

        if (wirdVerwendet)
        {
            return false;
        }

        // Cascade: Die Antwortoptionen werden von der Datenbank mitgelöscht.
        _context.Fragen.Remove(frage);
        await _context.SaveChangesAsync();
        return true;
    }

    // Freitextfragen haben keine Antwortoptionen, Multiple-Choice-Fragen keine Musterlösung.
    private static void BereinigeNachTyp(Frage frage)
    {
        frage.Fragetext = frage.Fragetext.Trim();

        if (frage.Typ == Fragetyp.Freitext)
        {
            frage.Antwortoptionen.Clear();
        }
        else
        {
            frage.MusterloesungFreitext = null;
        }
    }
}
