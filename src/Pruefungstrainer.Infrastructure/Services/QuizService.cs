using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Core.Models;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="IQuizService"/>.
///
/// Bewertungsregeln:
/// - Multiple Choice: volle Punkte nur, wenn genau die richtigen Optionen angekreuzt sind
///   ("Alles-oder-nichts" – verhindert, dass Raten durch Ankreuzen aller Optionen belohnt wird).
/// - Freitext: kann nicht zuverlässig automatisch bewertet werden. Er startet mit 0 Punkten;
///   der Teilnehmer vergleicht danach mit der Musterlösung und kann ihn als richtig werten.
/// </summary>
public class QuizService : IQuizService
{
    private const int MaxLaengeAntwort = 1000; // entspricht [MaxLength(1000)] an GegebeneAntwort

    private readonly ApplicationDbContext _context;

    public QuizService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Frage>> ErstelleFragenAsync(int? themengebietId, int? lernpfadId, int anzahl)
    {
        List<int> frageIds;

        if (lernpfadId.HasValue)
        {
            frageIds = await _context.LernpfadElemente
                .Where(e => e.LernpfadId == lernpfadId.Value && e.FrageId != null)
                .Select(e => e.FrageId!.Value)
                .ToListAsync();
        }
        else
        {
            var abfrage = _context.Fragen.AsQueryable();
            if (themengebietId.HasValue)
            {
                abfrage = abfrage.Where(f => f.ThemengebietId == themengebietId.Value);
            }

            frageIds = await abfrage.Select(f => f.Id).ToListAsync();
        }

        // Zufällige Reihenfolge, damit sich Durchläufe unterscheiden und man nicht auswendig lernt.
        var zufaellig = frageIds.ToArray();
        Random.Shared.Shuffle(zufaellig);

        return await LadeFragenAsync(zufaellig.Take(anzahl).ToList());
    }

    public async Task<List<Frage>> LadeFragenAsync(IReadOnlyList<int> frageIds)
    {
        var fragen = await _context.Fragen
            .AsNoTracking()
            .Include(f => f.Antwortoptionen)
            .Include(f => f.Themengebiet)
            .Where(f => frageIds.Contains(f.Id))
            .ToListAsync();

        // Die Datenbank liefert in beliebiger Reihenfolge – wir stellen die gewünschte wieder her.
        return frageIds
            .Select(id => fragen.FirstOrDefault(f => f.Id == id))
            .OfType<Frage>()
            .ToList();
    }

    public async Task<int> AuswertenAsync(int benutzerId, IReadOnlyList<int> frageIds, int? lernpfadId, IEnumerable<QuizAntwort> antworten)
    {
        // Maßstab sind die Fragen, die wirklich gestellt wurden (aus der Sitzung, nicht aus dem Formular).
        // So kann niemand durch Weglassen von Fragen sein Ergebnis verbessern.
        var fragen = await LadeFragenAsync(frageIds);
        var antwortProFrage = antworten
            .GroupBy(a => a.FrageId)
            .ToDictionary(g => g.Key, g => g.First());

        var ergebnis = new Pruefungsergebnis
        {
            ApplicationUserId = benutzerId,
            LernpfadId = lernpfadId,
            DurchgefuehrtAm = DateTime.UtcNow
        };

        foreach (var frage in fragen)
        {
            antwortProFrage.TryGetValue(frage.Id, out var antwort);
            ergebnis.Details.Add(BewerteFrage(frage, antwort));
        }

        BerechneGesamtergebnis(ergebnis, fragen.Sum(f => f.Punkte));

        _context.Pruefungsergebnisse.Add(ergebnis);
        await _context.SaveChangesAsync();
        return ergebnis.Id;
    }

    public async Task<Pruefungsergebnis?> GetErgebnisAsync(int ergebnisId, int benutzerId)
    {
        return await _context.Pruefungsergebnisse
            .AsNoTracking()
            .Include(p => p.Lernpfad)
            .Include(p => p.Details)
                .ThenInclude(d => d.Frage)
                    .ThenInclude(f => f.Antwortoptionen)
            .Include(p => p.Details)
                .ThenInclude(d => d.Frage)
                    .ThenInclude(f => f.Themengebiet)
            .AsSplitQuery()
            // Nur eigene Ergebnisse – sonst könnte man über die Id fremde Ergebnisse ansehen.
            .FirstOrDefaultAsync(p => p.Id == ergebnisId && p.ApplicationUserId == benutzerId);
    }

    public async Task<List<Pruefungsergebnis>> GetErgebnisseAsync(int benutzerId)
    {
        return await _context.Pruefungsergebnisse
            .AsNoTracking()
            .Include(p => p.Lernpfad)
            .Where(p => p.ApplicationUserId == benutzerId)
            .OrderByDescending(p => p.DurchgefuehrtAm)
            .ToListAsync();
    }

    public async Task<bool> FreitextAlsRichtigWertenAsync(int detailId, int benutzerId)
    {
        var detail = await _context.PruefungsergebnisFragen
            .Include(d => d.Frage)
            .Include(d => d.Pruefungsergebnis)
                .ThenInclude(p => p.Details)
            .FirstOrDefaultAsync(d => d.Id == detailId && d.Pruefungsergebnis.ApplicationUserId == benutzerId);

        // Nur unbewertete Freitextantworten mit tatsächlich eingegebenem Text dürfen gewertet werden.
        if (detail is null ||
            detail.Frage.Typ != Fragetyp.Freitext ||
            detail.IstKorrekt ||
            string.IsNullOrWhiteSpace(detail.GegebeneAntwort))
        {
            return false;
        }

        detail.IstKorrekt = true;
        detail.ErreichtePunkte = detail.Frage.Punkte;

        var ergebnis = detail.Pruefungsergebnis;
        BerechneGesamtergebnis(ergebnis, ergebnis.MaxPunkte);

        await _context.SaveChangesAsync();
        return true;
    }

    private static PruefungsergebnisFrage BewerteFrage(Frage frage, QuizAntwort? antwort)
    {
        var detail = new PruefungsergebnisFrage { FrageId = frage.Id };

        if (frage.Typ == Fragetyp.MultipleChoice)
        {
            var gewaehlt = antwort?.GewaehlteOptionIds.ToHashSet() ?? new HashSet<int>();
            var richtig = frage.Antwortoptionen.Where(a => a.IstKorrekt).Select(a => a.Id).ToHashSet();

            // Nur Ids berücksichtigen, die wirklich zu dieser Frage gehören.
            gewaehlt.IntersectWith(frage.Antwortoptionen.Select(a => a.Id));

            detail.IstKorrekt = gewaehlt.Count > 0 && gewaehlt.SetEquals(richtig);
            detail.ErreichtePunkte = detail.IstKorrekt ? frage.Punkte : 0;

            var texte = frage.Antwortoptionen.Where(a => gewaehlt.Contains(a.Id)).Select(a => a.Text);
            detail.GegebeneAntwort = Kuerzen(string.Join("; ", texte));
        }
        else
        {
            detail.GegebeneAntwort = Kuerzen(antwort?.Freitext?.Trim());
            detail.IstKorrekt = false;
            detail.ErreichtePunkte = 0;
        }

        return detail;
    }

    private static void BerechneGesamtergebnis(Pruefungsergebnis ergebnis, int maxPunkte)
    {
        ergebnis.MaxPunkte = maxPunkte;
        ergebnis.ErreichtePunkte = ergebnis.Details.Sum(d => d.ErreichtePunkte);
        ergebnis.Bestanden = maxPunkte > 0 &&
            ergebnis.ErreichtePunkte * 100 >= maxPunkte * IQuizService.BestehensgrenzeProzent;
    }

    private static string? Kuerzen(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        return text.Length <= MaxLaengeAntwort ? text : text[..MaxLaengeAntwort];
    }
}
