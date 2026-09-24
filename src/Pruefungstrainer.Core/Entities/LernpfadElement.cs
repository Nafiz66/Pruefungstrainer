using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pruefungstrainer.Core.Entities;

/// <summary>
/// Verknüpft einen <see cref="Entities.Lernpfad"/> mit genau einer Karteikarte
/// oder Frage und definiert deren Reihenfolge innerhalb des Pfads.
/// </summary>
public class LernpfadElement
{
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Lernpfad))]
    public int LernpfadId { get; set; }

    public Lernpfad Lernpfad { get; set; } = null!;

    /// <summary>Position innerhalb des Lernpfads (aufsteigend, z.B. 1, 2, 3, ...).</summary>
    [Required]
    public int Reihenfolge { get; set; }

    [Required]
    public ElementTyp Typ { get; set; }

    // Genau eine der beiden folgenden Referenzen ist gesetzt, abhängig von Typ.
    [ForeignKey(nameof(Karteikarte))]
    public int? KarteikarteId { get; set; }

    public Karteikarte? Karteikarte { get; set; }

    [ForeignKey(nameof(Frage))]
    public int? FrageId { get; set; }

    public Frage? Frage { get; set; }
}
