using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Core.Interfaces;

/// <summary>
/// Benutzer- und Rollenverwaltung: Anmelden, Registrieren, Verwalten.
/// </summary>
public interface IBenutzerService
{
    /// <summary>Alle Benutzer, sortiert nach Nachname und Vorname.</summary>
    Task<List<ApplicationUser>> GetAllAsync();

    /// <summary>Prüft E-Mail und Passwort. Gibt den Benutzer zurück oder null, wenn die Anmeldung fehlschlägt.</summary>
    Task<ApplicationUser?> AnmeldenAsync(string email, string passwort);

    Task<bool> EmailExistsAsync(string email);

    /// <summary>Legt einen neuen Benutzer an. Das Passwort wird nur als Hash gespeichert, nie im Klartext.</summary>
    Task<ApplicationUser> RegistrierenAsync(string vorname, string nachname, string email, string passwort, UserRole rolle);

    /// <summary>Löscht einen Benutzer samt Lernfortschritt und Ergebnissen. False, wenn er nicht existiert.</summary>
    Task<bool> DeleteAsync(int id);
}
