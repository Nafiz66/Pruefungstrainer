using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Infrastructure.Data;

namespace Pruefungstrainer.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="IBenutzerService"/>.
/// Passwörter werden mit dem PasswordHasher von ASP.NET Core Identity gehasht
/// (PBKDF2 mit Salt und vielen Iterationen) – nie im Klartext gespeichert.
/// </summary>
public class BenutzerService : IBenutzerService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public BenutzerService(ApplicationDbContext context, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<ApplicationUser>> GetAllAsync()
    {
        return await _context.Benutzer
            .AsNoTracking()
            .OrderBy(b => b.Nachname)
            .ThenBy(b => b.Vorname)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> AnmeldenAsync(string email, string passwort)
    {
        var normalisiert = NormalisiereEmail(email);
        var benutzer = await _context.Benutzer.FirstOrDefaultAsync(b => b.Email == normalisiert);
        if (benutzer is null)
        {
            return null;
        }

        var ergebnis = _passwordHasher.VerifyHashedPassword(benutzer, benutzer.PasswordHash, passwort);
        if (ergebnis == PasswordVerificationResult.Failed)
        {
            return null;
        }

        // Falls der Hash mit einem älteren, schwächeren Verfahren erzeugt wurde: direkt erneuern.
        if (ergebnis == PasswordVerificationResult.SuccessRehashNeeded)
        {
            benutzer.PasswordHash = _passwordHasher.HashPassword(benutzer, passwort);
            await _context.SaveChangesAsync();
        }

        return benutzer;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalisiert = NormalisiereEmail(email);
        return await _context.Benutzer.AnyAsync(b => b.Email == normalisiert);
    }

    public async Task<ApplicationUser> RegistrierenAsync(string vorname, string nachname, string email, string passwort, UserRole rolle)
    {
        var benutzer = new ApplicationUser
        {
            Vorname = vorname.Trim(),
            Nachname = nachname.Trim(),
            Email = NormalisiereEmail(email),
            Rolle = rolle,
            ErstelltAm = DateTime.UtcNow
        };
        benutzer.PasswordHash = _passwordHasher.HashPassword(benutzer, passwort);

        _context.Benutzer.Add(benutzer);
        await _context.SaveChangesAsync();
        return benutzer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var benutzer = await _context.Benutzer.FindAsync(id);
        if (benutzer is null)
        {
            return false;
        }

        // Cascade (siehe ApplicationUserConfiguration): Fortschritte und Ergebnisse werden mitgelöscht.
        _context.Benutzer.Remove(benutzer);
        await _context.SaveChangesAsync();
        return true;
    }

    // E-Mail-Adressen einheitlich klein und ohne Leerzeichen speichern,
    // damit "Max@Test.de" und "max@test.de " als dieselbe Adresse gelten.
    private static string NormalisiereEmail(string email) => email.Trim().ToLowerInvariant();
}
