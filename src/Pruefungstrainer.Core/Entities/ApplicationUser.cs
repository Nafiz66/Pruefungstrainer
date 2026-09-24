using System.ComponentModel.DataAnnotations;

namespace Pruefungstrainer.Core.Entities;

/// <summary>
/// Repräsentiert einen Benutzer der Plattform – entweder eine Lehrkraft oder einen Teilnehmer.
/// </summary>
public class ApplicationUser
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Vorname { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Nachname { get; set; } = string.Empty;

    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gehashtes Passwort – niemals Klartext speichern.</summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Rolle { get; set; } = UserRole.Teilnehmer;

    public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<KarteikartenFortschritt> KarteikartenFortschritte { get; set; }
        = new List<KarteikartenFortschritt>();

    public ICollection<Pruefungsergebnis> Pruefungsergebnisse { get; set; }
        = new List<Pruefungsergebnis>();
}
