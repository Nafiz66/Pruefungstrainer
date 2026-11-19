using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Core.Interfaces;

/// <summary>
/// Verwaltung der Prüfungsfragen inklusive ihrer Antwortoptionen.
/// </summary>
public interface IFrageService
{
    /// <summary>Alle Fragen (optional nur eines Themengebiets), inklusive Themengebiet.</summary>
    Task<List<Frage>> GetAllAsync(int? themengebietId = null);

    /// <summary>Eine Frage inklusive Antwortoptionen und Themengebiet oder null.</summary>
    Task<Frage?> GetByIdAsync(int id);

    /// <summary>Speichert die Frage zusammen mit den Antwortoptionen in ihrer Collection.</summary>
    Task CreateAsync(Frage frage);

    /// <summary>Übernimmt die Felder und ersetzt die Antwortoptionen komplett.</summary>
    Task UpdateAsync(Frage frage);

    /// <summary>False, wenn die Frage nicht existiert oder schon in Lernpfaden/Prüfungsergebnissen verwendet wird.</summary>
    Task<bool> DeleteAsync(int id);
}
