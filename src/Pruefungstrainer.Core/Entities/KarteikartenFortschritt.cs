using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruefungstrainingPlattform.Core.Entities;

/// <summary>
/// Speichert pro Teilnehmer und Karteikarte den aktuellen Lernstatus.
/// Fungiert als Verknüpfungstabelle zwischen <see cref="ApplicationUser"/> und <see cref="Karteikarte"/>.
/// </summary>
public class KarteikartenFortschritt
{
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(ApplicationUser))]
    public int ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Karteikarte))]
    public int KarteikarteId { get; set; }

    public Karteikarte Karteikarte { get; set; } = null!;

    [Required]
    public KarteikartenStatus Status { get; set; } = KarteikartenStatus.Neu;

    /// <summary>Anzahl der bisherigen Wiederholungen dieser Karteikarte.</summary>
    public int WiederholungenAnzahl { get; set; }

    public DateTime? LetzteBearbeitungAm { get; set; }
}
