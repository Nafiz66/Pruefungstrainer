using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruefungstrainingPlattform.Core.Entities;

/// <summary>
/// Eine einzelne Antwortoption einer Multiple-Choice-<see cref="Entities.Frage"/>.
/// </summary>
public class Antwortoption
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    public bool IstKorrekt { get; set; }

    [Required]
    [ForeignKey(nameof(Frage))]
    public int FrageId { get; set; }

    public Frage Frage { get; set; } = null!;
}
