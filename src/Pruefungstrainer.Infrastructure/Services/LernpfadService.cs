using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="ILernpfadService"/> mit EF Core.
/// </summary>
public class LernpfadService : ILernpfadService
{
    private readonly ApplicationDbContext _context;

    public LernpfadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Lernpfad>> GetAllAsync()
    {
        return await _context.Lernpfade
            .AsNoTracking()
            .Include(l => l.Themengebiet)
            .Include(l => l.Elemente)
            .OrderBy(l => l.Themengebiet.Name)
            .ThenBy(l => l.Titel)
            .ToListAsync();
    }

    public async Task<Lernpfad?> GetByIdAsync(int id)
    {
        return await _context.Lernpfade
            .AsNoTracking()
            .Include(l => l.Themengebiet)
            .Include(l => l.Elemente.OrderBy(e => e.Reihenfolge))
                .ThenInclude(e => e.Karteikarte)
            .Include(l => l.Elemente)
                .ThenInclude(e => e.Frage)
                    .ThenInclude(f => f!.Antwortoptionen)
            .AsSplitQuery()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task CreateAsync(Lernpfad lernpfad)
    {
        lernpfad.Titel = lernpfad.Titel.Trim();

        _context.Lernpfade.Add(lernpfad);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Lernpfad lernpfad)
    {
        var vorhanden = await _context.Lernpfade.FindAsync(lernpfad.Id)
            ?? throw new KeyNotFoundException($"Lernpfad {lernpfad.Id} nicht gefunden.");

        vorhanden.Titel = lernpfad.Titel.Trim();
        vorhanden.Beschreibung = lernpfad.Beschreibung;
        vorhanden.ThemengebietId = lernpfad.ThemengebietId;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var lernpfad = await _context.Lernpfade.FindAsync(id);
        if (lernpfad is null)
        {
            return false;
        }

        // Cascade: Elemente werden mitgelöscht. SetNull: Prüfungsergebnisse bleiben als Historie erhalten.
        _context.Lernpfade.Remove(lernpfad);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> ElementHinzufuegenAsync(int lernpfadId, ElementTyp typ, int inhaltId)
    {
        var lernpfad = await _context.Lernpfade
            .Include(l => l.Elemente)
            .FirstOrDefaultAsync(l => l.Id == lernpfadId);

        if (lernpfad is null)
        {
            return "Der Lernpfad wurde nicht gefunden.";
        }

        // Der Inhalt muss existieren und zum Themengebiet des Lernpfads gehören.
        int? themengebietDesInhalts = typ == ElementTyp.Karteikarte
            ? await _context.Karteikarten.Where(k => k.Id == inhaltId).Select(k => (int?)k.ThemengebietId).FirstOrDefaultAsync()
            : await _context.Fragen.Where(f => f.Id == inhaltId).Select(f => (int?)f.ThemengebietId).FirstOrDefaultAsync();

        if (themengebietDesInhalts is null)
        {
            return "Der gewählte Inhalt wurde nicht gefunden.";
        }

        if (themengebietDesInhalts != lernpfad.ThemengebietId)
        {
            return "Der Inhalt gehört zu einem anderen Themengebiet als der Lernpfad.";
        }

        var schonEnthalten = lernpfad.Elemente.Any(e =>
            (typ == ElementTyp.Karteikarte && e.KarteikarteId == inhaltId) ||
            (typ == ElementTyp.Frage && e.FrageId == inhaltId));

        if (schonEnthalten)
        {
            return "Dieser Inhalt ist bereits im Lernpfad enthalten.";
        }

        lernpfad.Elemente.Add(new LernpfadElement
        {
            Typ = typ,
            KarteikarteId = typ == ElementTyp.Karteikarte ? inhaltId : null,
            FrageId = typ == ElementTyp.Frage ? inhaltId : null,
            // Neues Element kommt ans Ende.
            Reihenfolge = lernpfad.Elemente.Count == 0 ? 1 : lernpfad.Elemente.Max(e => e.Reihenfolge) + 1
        });

        await _context.SaveChangesAsync();
        return null;
    }

    public async Task ElementEntfernenAsync(int elementId)
    {
        var element = await _context.LernpfadElemente.FindAsync(elementId);
        if (element is null)
        {
            return;
        }

        await using var transaktion = await _context.Database.BeginTransactionAsync();

        _context.LernpfadElemente.Remove(element);
        await _context.SaveChangesAsync();

        // Lücke schließen: 1, 2, 4, 5 → 1, 2, 3, 4.
        // Einzeln speichern, weil (LernpfadId, Reihenfolge) eindeutig ist und SQLite
        // die Eindeutigkeit nach jeder einzelnen Zeile prüft.
        var nachfolger = await _context.LernpfadElemente
            .Where(e => e.LernpfadId == element.LernpfadId && e.Reihenfolge > element.Reihenfolge)
            .OrderBy(e => e.Reihenfolge)
            .ToListAsync();

        foreach (var e in nachfolger)
        {
            e.Reihenfolge--;
            await _context.SaveChangesAsync();
        }

        await transaktion.CommitAsync();
    }

    public async Task ElementVerschiebenAsync(int elementId, bool nachOben)
    {
        var element = await _context.LernpfadElemente.FindAsync(elementId);
        if (element is null)
        {
            return;
        }

        var nachbar = nachOben
            ? await _context.LernpfadElemente
                .Where(e => e.LernpfadId == element.LernpfadId && e.Reihenfolge < element.Reihenfolge)
                .OrderByDescending(e => e.Reihenfolge)
                .FirstOrDefaultAsync()
            : await _context.LernpfadElemente
                .Where(e => e.LernpfadId == element.LernpfadId && e.Reihenfolge > element.Reihenfolge)
                .OrderBy(e => e.Reihenfolge)
                .FirstOrDefaultAsync();

        if (nachbar is null)
        {
            return; // Schon ganz oben bzw. ganz unten.
        }

        // Tauschen über einen Zwischenwert, sonst verletzt der erste Schritt den Unique-Index.
        // Die Transaktion sorgt dafür, dass entweder alle drei Schritte gelingen oder keiner.
        await using var transaktion = await _context.Database.BeginTransactionAsync();

        var positionElement = element.Reihenfolge;
        var positionNachbar = nachbar.Reihenfolge;

        element.Reihenfolge = -1;
        await _context.SaveChangesAsync();

        nachbar.Reihenfolge = positionElement;
        await _context.SaveChangesAsync();

        element.Reihenfolge = positionNachbar;
        await _context.SaveChangesAsync();

        await transaktion.CommitAsync();
    }
}
