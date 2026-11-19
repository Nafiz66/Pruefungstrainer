using System.ComponentModel.DataAnnotations;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Web.Models;

/// <summary>Formulardaten der Anmeldung.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Bitte die E-Mail-Adresse eingeben.")]
    [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
    [Display(Name = "E-Mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte das Passwort eingeben.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string Passwort { get; set; } = string.Empty;

    /// <summary>Die Seite, die vor dem Login aufgerufen wurde – dorthin geht es danach zurück.</summary>
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// Formulardaten zum Anlegen eines Benutzers – für die Selbstregistrierung (immer Teilnehmer)
/// und für die Benutzerverwaltung der Lehrkraft (Rolle wählbar).
/// </summary>
public class BenutzerFormViewModel
{
    [Required(ErrorMessage = "Bitte den Vornamen eingeben.")]
    [MaxLength(100)]
    public string Vorname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte den Nachnamen eingeben.")]
    [MaxLength(100)]
    public string Nachname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte die E-Mail-Adresse eingeben.")]
    [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
    [MaxLength(256)]
    [Display(Name = "E-Mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte ein Passwort eingeben.")]
    [MinLength(8, ErrorMessage = "Das Passwort muss mindestens 8 Zeichen lang sein.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string Passwort { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte das Passwort wiederholen.")]
    [Compare(nameof(Passwort), ErrorMessage = "Die Passwörter stimmen nicht überein.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort wiederholen")]
    public string PasswortWiederholung { get; set; } = string.Empty;

    public UserRole Rolle { get; set; } = UserRole.Teilnehmer;
}
