using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Core.Interfaces;

/// <summary>
/// Verwaltung von Lernpfaden und ihren Elementen (Karteikarten und Fragen in fester Reihenfolge).
/// </summary>
public interface ILernpfadService
{
    /// <summary>Alle Lernpfade inklusive Themengebiet und Elementen (für die Anzahl).</summary>
    Task<List<Lernpfad>> GetAllAsync();

    /// <summary>Ein Lernpfad mit Themengebiet und allen Elementen (sortiert, inkl. Karteikarte/Frage/Antwortoptionen).</summary>
    Task<Lernpfad?> GetByIdAsync(int id);

    Task CreateAsync(Lernpfad lernpfad);

    Task UpdateAsync(Lernpfad lernpfad);

    /// <summary>Löscht den Lernpfad. Seine Elemente werden mitgelöscht, Prüfungsergebnisse bleiben erhalten.</summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Hängt eine Karteikarte oder Frage hinten an. Gibt eine Fehlermeldung zurück oder null bei Erfolg.
    /// </summary>
    Task<string?> ElementHinzufuegenAsync(int lernpfadId, ElementTyp typ, int inhaltId);

    /// <summary>Entfernt ein Element und nummeriert die restlichen lückenlos neu.</summary>
    Task ElementEntfernenAsync(int elementId);

    /// <summary>Tauscht ein Element mit seinem Vorgänger (nachOben) bzw. Nachfolger.</summary>
    Task ElementVerschiebenAsync(int elementId, bool nachOben);
}
