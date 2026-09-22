using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruefungstrainingPlattform.Core.Entities;

/// <summary>
/// Ein strukturierter Lernpfad innerhalb eines Themengebiets, bestehend aus
/// einer geordneten Abfolge von Karteikarten und/oder Fragen (<see cref="LernpfadElement"/>).
/// </summary>
public class Lernpfad
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Titel { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Beschreibung { get; set; }

    [Required]
    [ForeignKey(nameof(Themengebiet))]
    public int ThemengebietId { get; set; }

    public Themengebiet Themengebiet { get; set; } = null!;

    // Navigation Properties
    public ICollection<LernpfadElement> Elemente { get; set; } = new List<LernpfadElement>();

    public ICollection<Pruefungsergebnis> Pruefungsergebnisse { get; set; } = new List<Pruefungsergebnis>();
}
