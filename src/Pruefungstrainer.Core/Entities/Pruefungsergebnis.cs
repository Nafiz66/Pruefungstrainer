using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruefungstrainingPlattform.Core.Entities;

/// <summary>
/// Das Gesamtergebnis eines von einem Teilnehmer durchgeführten Quiz- oder
/// Prüfungssimulationsdurchlaufs. Die Einzelantworten liegen in <see cref="Details"/>.
/// </summary>
public class Pruefungsergebnis
{
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(ApplicationUser))]
    public int ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = null!;

    /// <summary>Optionaler Bezug zu einem Lernpfad, falls der Durchlauf im Rahmen eines Lernpfads erfolgte.</summary>
    [ForeignKey(nameof(Lernpfad))]
    public int? LernpfadId { get; set; }

    public Lernpfad? Lernpfad { get; set; }

    [Required]
    public DateTime DurchgefuehrtAm { get; set; } = DateTime.UtcNow;

    public int ErreichtePunkte { get; set; }

    public int MaxPunkte { get; set; }

    public bool Bestanden { get; set; }

    // Navigation Property
    public ICollection<PruefungsergebnisFrage> Details { get; set; } = new List<PruefungsergebnisFrage>();
}
