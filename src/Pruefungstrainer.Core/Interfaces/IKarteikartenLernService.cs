using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Core.Interfaces;

/// <summary>
/// Lernen mit Karteikarten aus Sicht eines Teilnehmers (Fortschritt je Benutzer und Karte).
/// </summary>
public interface IKarteikartenLernService
{
    /// <summary>
    /// Die nächste noch nicht gelernte Karte eines Themengebiets für diesen Benutzer.
    /// Zuerst neue Karten, dann die am längsten nicht mehr wiederholten. Null, wenn alle gelernt sind.
    /// </summary>
    Task<Karteikarte?> NaechsteKarteAsync(int benutzerId, int themengebietId);

    /// <summary>Speichert "gewusst" bzw. "nicht gewusst" im KarteikartenFortschritt.</summary>
    Task BewertenAsync(int benutzerId, int karteikarteId, bool gewusst);

    /// <summary>Setzt alle Karten eines Themengebiets für diesen Benutzer wieder auf "Neu".</summary>
    Task ZuruecksetzenAsync(int benutzerId, int themengebietId);

    /// <summary>Wie viele Karten des Themengebiets hat der Benutzer schon gelernt?</summary>
    Task<(int Gelernt, int Gesamt)> ZaehleAsync(int benutzerId, int themengebietId);
}
