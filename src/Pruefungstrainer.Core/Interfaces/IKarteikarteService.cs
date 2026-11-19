using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Core.Interfaces;

/// <summary>
/// Verwaltung der Karteikarten durch die Lehrkraft.
/// </summary>
public interface IKarteikarteService
{
    /// <summary>Alle Karteikarten (optional nur eines Themengebiets), inklusive Themengebiet.</summary>
    Task<List<Karteikarte>> GetAllAsync(int? themengebietId = null);

    Task<Karteikarte?> GetByIdAsync(int id);

    Task CreateAsync(Karteikarte karteikarte);

    Task UpdateAsync(Karteikarte karteikarte);

    /// <summary>False, wenn die Karte nicht existiert oder noch in einem Lernpfad steckt.</summary>
    Task<bool> DeleteAsync(int id);
}
