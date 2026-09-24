using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pruefungstrainer.Core.Entities;

/// <summary>
/// Detailergebnis einer einzelnen Frage innerhalb eines <see cref="Entities.Pruefungsergebnis"/>.
/// Speichert die gegebene Antwort sowie deren Bewertung.
/// </summary>
public class PruefungsergebnisFrage
{
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Pruefungsergebnis))]
    public int PruefungsergebnisId { get; set; }

    public Pruefungsergebnis Pruefungsergebnis { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Frage))]
    public int FrageId { get; set; }

    public Frage Frage { get; set; } = null!;

    /// <summary>
    /// Bei Multiple-Choice: ID(s) der gewählten Antwortoption(en) (z.B. kommasepariert).
    /// Bei Freitext: der eingegebene Antworttext.
    /// </summary>
    [MaxLength(1000)]
    public string? GegebeneAntwort { get; set; }

    public bool IstKorrekt { get; set; }

    public int ErreichtePunkte { get; set; }
}
