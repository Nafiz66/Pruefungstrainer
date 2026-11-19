using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="IKarteikartenLernService"/>.
/// Der Fortschritt wird pro Benutzer und Karte in KarteikartenFortschritt gespeichert.
/// </summary>
public class KarteikartenLernService : IKarteikartenLernService
{
    private readonly ApplicationDbContext _context;

    public KarteikartenLernService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Karteikarte?> NaechsteKarteAsync(int benutzerId, int themengebietId)
    {
        var karten = await _context.Karteikarten
            .AsNoTracking()
            .Where(k => k.ThemengebietId == themengebietId)
            .ToListAsync();

        var fortschritte = await LadeFortschritteAsync(benutzerId, themengebietId);

        // Noch nicht gelernte Karten: zuerst nie bearbeitete, dann die am längsten nicht wiederholten.
        return karten
            .Select(k => new { Karte = k, Fortschritt = fortschritte.GetValueOrDefault(k.Id) })
            .Where(x => x.Fortschritt?.Status != KarteikartenStatus.Gelernt)
            .OrderBy(x => x.Fortschritt?.LetzteBearbeitungAm ?? DateTime.MinValue)
            .ThenBy(x => x.Karte.Id)
            .Select(x => x.Karte)
            .FirstOrDefault();
    }

    public async Task BewertenAsync(int benutzerId, int karteikarteId, bool gewusst)
    {
        var fortschritt = await _context.KarteikartenFortschritte
            .FirstOrDefaultAsync(f => f.ApplicationUserId == benutzerId && f.KarteikarteId == karteikarteId);

        if (fortschritt is null)
        {
            // Erster Kontakt mit dieser Karte: Fortschrittsdatensatz anlegen.
            fortschritt = new KarteikartenFortschritt
            {
                ApplicationUserId = benutzerId,
                KarteikarteId = karteikarteId
            };
            _context.KarteikartenFortschritte.Add(fortschritt);
        }

        fortschritt.Status = gewusst ? KarteikartenStatus.Gelernt : KarteikartenStatus.InBearbeitung;
        fortschritt.WiederholungenAnzahl++;
        fortschritt.LetzteBearbeitungAm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ZuruecksetzenAsync(int benutzerId, int themengebietId)
    {
        var fortschritte = await _context.KarteikartenFortschritte
            .Where(f => f.ApplicationUserId == benutzerId && f.Karteikarte.ThemengebietId == themengebietId)
            .ToListAsync();

        foreach (var fortschritt in fortschritte)
        {
            fortschritt.Status = KarteikartenStatus.Neu;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<(int Gelernt, int Gesamt)> ZaehleAsync(int benutzerId, int themengebietId)
    {
        var gesamt = await _context.Karteikarten.CountAsync(k => k.ThemengebietId == themengebietId);
        var gelernt = await _context.KarteikartenFortschritte.CountAsync(f =>
            f.ApplicationUserId == benutzerId &&
            f.Karteikarte.ThemengebietId == themengebietId &&
            f.Status == KarteikartenStatus.Gelernt);

        return (gelernt, gesamt);
    }

    private async Task<Dictionary<int, KarteikartenFortschritt>> LadeFortschritteAsync(int benutzerId, int themengebietId)
    {
        return await _context.KarteikartenFortschritte
            .AsNoTracking()
            .Where(f => f.ApplicationUserId == benutzerId && f.Karteikarte.ThemengebietId == themengebietId)
            .ToDictionaryAsync(f => f.KarteikarteId);
    }
}
