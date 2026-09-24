using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pruefungstrainer.Core.Entities;

/// <summary>
/// Eine Karteikarte mit Frage- und Antworttext, einem Themengebiet zugeordnet.
/// </summary>
public class Karteikarte
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string Frage { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Antwort { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Themengebiet))]
    public int ThemengebietId { get; set; }

    public Themengebiet Themengebiet { get; set; } = null!;

    // Navigation Properties
    public ICollection<LernpfadElement> LernpfadElemente { get; set; } = new List<LernpfadElement>();

    public ICollection<KarteikartenFortschritt> Fortschritte { get; set; } = new List<KarteikartenFortschritt>();
}
