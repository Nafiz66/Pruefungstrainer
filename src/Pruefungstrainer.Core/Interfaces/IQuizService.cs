using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Models;

namespace Pruefungstrainer.Core.Interfaces;

/// <summary>
/// Quiz- und Prüfungssimulationslogik: Fragen zusammenstellen, Antworten bewerten, Ergebnisse speichern.
/// </summary>
public interface IQuizService
{
    /// <summary>Prozentsatz, ab dem eine Einheit als bestanden gilt (wie bei der IHK: 50 %).</summary>
    const int BestehensgrenzeProzent = 50;

    /// <summary>
    /// Stellt zufällig Fragen zusammen: aus einem Themengebiet, aus einem Lernpfad oder (beides null) aus allen.
    /// </summary>
    Task<List<Frage>> ErstelleFragenAsync(int? themengebietId, int? lernpfadId, int anzahl);

    /// <summary>Lädt Fragen inklusive Antwortoptionen in der Reihenfolge der übergebenen Ids.</summary>
    Task<List<Frage>> LadeFragenAsync(IReadOnlyList<int> frageIds);

    /// <summary>
    /// Bewertet die Antworten und speichert ein Prüfungsergebnis. Maßstab sind die gestellten Fragen
    /// (frageIds) – unbeantwortete Fragen zählen mit 0 Punkten. Gibt die Id des Ergebnisses zurück.
    /// </summary>
    Task<int> AuswertenAsync(int benutzerId, IReadOnlyList<int> frageIds, int? lernpfadId, IEnumerable<QuizAntwort> antworten);

    /// <summary>Ein Ergebnis inklusive Details, Fragen und Antwortoptionen – nur wenn es diesem Benutzer gehört.</summary>
    Task<Pruefungsergebnis?> GetErgebnisAsync(int ergebnisId, int benutzerId);

    /// <summary>Alle Ergebnisse eines Benutzers, neueste zuerst.</summary>
    Task<List<Pruefungsergebnis>> GetErgebnisseAsync(int benutzerId);

    /// <summary>
    /// Selbsteinschätzung bei Freitext: Der Teilnehmer vergleicht mit der Musterlösung und wertet
    /// seine Antwort als richtig. Punkte und "Bestanden" werden neu berechnet.
    /// </summary>
    Task<bool> FreitextAlsRichtigWertenAsync(int detailId, int benutzerId);
}
