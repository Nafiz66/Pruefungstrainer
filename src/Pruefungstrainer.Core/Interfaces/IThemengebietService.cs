using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Core.Interfaces;

/// <summary>
/// Vertrag für alle Operationen rund um Themengebiete.
/// Der Controller kennt nur dieses Interface, nicht die Datenbank.
/// </summary>
public interface IThemengebietService
{
    /// <summary>Alle Themengebiete, alphabetisch nach Name.</summary>
    Task<List<Themengebiet>> GetAllAsync();

    /// <summary>Ein Themengebiet oder null, wenn die Id nicht existiert.</summary>
    Task<Themengebiet?> GetByIdAsync(int id);

    /// <summary>Prüft, ob der Name schon vergeben ist (optional ein Themengebiet ausnehmen, z.B. beim Bearbeiten).</summary>
    Task<bool> NameExistsAsync(string name, int? ausnahmeId = null);

    Task CreateAsync(Themengebiet themengebiet);

    Task UpdateAsync(Themengebiet themengebiet);

    /// <summary>Löscht das Themengebiet. Gibt false zurück, wenn noch Fragen, Karteikarten oder Lernpfade daran hängen.</summary>
    Task<bool> DeleteAsync(int id);
}
