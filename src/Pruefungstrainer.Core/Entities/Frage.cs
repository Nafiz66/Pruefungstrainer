using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pruefungstrainer.Core.Entities;

/// <summary>
/// Eine Frage für Quiz- oder Prüfungssimulationen. Je nach <see cref="Typ"/>
/// entweder Multiple-Choice (mit <see cref="Antwortoptionen"/>) oder Freitext
/// (mit optionaler Musterlösung zur manuellen/automatisierten Bewertung).
/// </summary>
public class Frage
{
    public int Id { get; set; }

    [Required, MaxLength(1000)]
    public string Fragetext { get; set; } = string.Empty;

    [Required]
    public Fragetyp Typ { get; set; }

    /// <summary>Maximal erreichbare Punktzahl für diese Frage.</summary>
    [Range(1, 100)]
    public int Punkte { get; set; } = 1;

    /// <summary>Nur relevant bei <see cref="Fragetyp.Freitext"/>: Musterlösung/Bewertungshinweis.</summary>
    [MaxLength(1000)]
    public string? MusterloesungFreitext { get; set; }

    [Required]
    [ForeignKey(nameof(Themengebiet))]
    public int ThemengebietId { get; set; }

    public Themengebiet Themengebiet { get; set; } = null!;

    // Navigation Properties
    public ICollection<Antwortoption> Antwortoptionen { get; set; } = new List<Antwortoption>();

    public ICollection<LernpfadElement> LernpfadElemente { get; set; } = new List<LernpfadElement>();

    public ICollection<PruefungsergebnisFrage> Ergebnisdetails { get; set; } = new List<PruefungsergebnisFrage>();
}
