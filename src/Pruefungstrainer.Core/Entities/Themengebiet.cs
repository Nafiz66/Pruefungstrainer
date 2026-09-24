using System.ComponentModel.DataAnnotations;

namespace Pruefungstrainer.Core.Entities;

/// <summary>
/// Ein fachliches Themengebiet (z.B. "Datenbanken – AP1"), dem Lernpfade,
/// Karteikarten und Fragen zugeordnet werden.
/// </summary>
public class Themengebiet
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Beschreibung { get; set; }

    /// <summary>Grobe Einordnung, z.B. "AP1" oder "AP2".</summary>
    [MaxLength(50)]
    public string? Kategorie { get; set; }

    // Navigation Properties
    public ICollection<Lernpfad> Lernpfade { get; set; } = new List<Lernpfad>();

    public ICollection<Karteikarte> Karteikarten { get; set; } = new List<Karteikarte>();

    public ICollection<Frage> Fragen { get; set; } = new List<Frage>();
}
