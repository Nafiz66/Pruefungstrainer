using Microsoft.AspNetCore.Identity;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Infrastructure.Data;

/// <summary>
/// Füllt eine leere Datenbank mit Beispieldaten, damit man direkt nach dem ersten Start
/// etwas sieht und sich anmelden kann. Jeder Block prüft für sich, ob schon Daten da sind,
/// damit auch eine ältere Datenbank nachträglich ergänzt wird.
/// Liegt in der Infrastructure, weil es Datenzugriff ist – nicht Aufgabe des Web-Projekts.
/// </summary>
public static class DbSeeder
{
    // Test-Zugänge für Entwicklung und Präsentation. Vor einem echten Einsatz ändern!
    public const string LehrkraftEmail = "lehrkraft@pruefungstrainer.local";
    public const string LehrkraftPasswort = "Lehrkraft123!";
    public const string TeilnehmerEmail = "teilnehmer@pruefungstrainer.local";
    public const string TeilnehmerPasswort = "Teilnehmer123!";

    public static void Seed(ApplicationDbContext db, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        SeedBenutzer(db, passwordHasher);
        SeedThemengebieteUndFragen(db);
        SeedKarteikarten(db);
        SeedLernpfad(db);
    }

    private static void SeedBenutzer(ApplicationDbContext db, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        if (db.Benutzer.Any())
        {
            return;
        }

        var lehrkraft = new ApplicationUser
        {
            Vorname = "Lea",
            Nachname = "Lehrkraft",
            Email = LehrkraftEmail,
            Rolle = UserRole.Lehrkraft
        };
        lehrkraft.PasswordHash = passwordHasher.HashPassword(lehrkraft, LehrkraftPasswort);

        var teilnehmer = new ApplicationUser
        {
            Vorname = "Tim",
            Nachname = "Teilnehmer",
            Email = TeilnehmerEmail,
            Rolle = UserRole.Teilnehmer
        };
        teilnehmer.PasswordHash = passwordHasher.HashPassword(teilnehmer, TeilnehmerPasswort);

        db.Benutzer.AddRange(lehrkraft, teilnehmer);
        db.SaveChanges();
    }

    private static void SeedThemengebieteUndFragen(ApplicationDbContext db)
    {
        if (db.Themengebiete.Any())
        {
            return;
        }

        db.Themengebiete.AddRange(
            new Themengebiet { Id = 1, Name = "C# Grundlagen", Kategorie = "AP1", Beschreibung = "Grundlagen der C#-Programmierung und OOP." },
            new Themengebiet { Id = 2, Name = "Datenbanken", Kategorie = "AP2", Beschreibung = "Relationale Datenbanken, SQL und Entity Framework." }
        );

        db.Fragen.AddRange(
            new Frage { Id = 1, Fragetext = "Welche Programmiersprache wird für ASP.NET Core häufig verwendet?", Typ = Fragetyp.MultipleChoice, Punkte = 2, ThemengebietId = 1 },
            new Frage { Id = 2, Fragetext = "Was beschreibt eine SQL-Tabelle am besten?", Typ = Fragetyp.MultipleChoice, Punkte = 3, ThemengebietId = 2 },
            new Frage { Id = 3, Fragetext = "Nenne ein Prinzip der objektorientierten Programmierung.", Typ = Fragetyp.Freitext, Punkte = 5, ThemengebietId = 1, MusterloesungFreitext = "Kapselung, Vererbung oder Polymorphie." }
        );

        db.Antwortoptionen.AddRange(
            new Antwortoption { Id = 1, Text = "C#", IstKorrekt = true, FrageId = 1 },
            new Antwortoption { Id = 2, Text = "Java", IstKorrekt = false, FrageId = 1 },
            new Antwortoption { Id = 3, Text = "Python", IstKorrekt = false, FrageId = 1 },
            new Antwortoption { Id = 4, Text = "Eine Sammlung von Datensätzen mit gleicher Struktur", IstKorrekt = true, FrageId = 2 },
            new Antwortoption { Id = 5, Text = "Ein Browser-Plugin", IstKorrekt = false, FrageId = 2 },
            new Antwortoption { Id = 6, Text = "Ein CSS-Stil", IstKorrekt = false, FrageId = 2 }
        );

        db.SaveChanges();
    }

    private static void SeedKarteikarten(ApplicationDbContext db)
    {
        if (db.Karteikarten.Any())
        {
            return;
        }

        var csharp = db.Themengebiete.FirstOrDefault(t => t.Name == "C# Grundlagen");
        var datenbanken = db.Themengebiete.FirstOrDefault(t => t.Name == "Datenbanken");
        if (csharp is null || datenbanken is null)
        {
            return;
        }

        db.Karteikarten.AddRange(
            new Karteikarte { ThemengebietId = csharp.Id, Frage = "Was ist eine Klasse?", Antwort = "Ein Bauplan für Objekte: Sie beschreibt Eigenschaften (Daten) und Methoden (Verhalten)." },
            new Karteikarte { ThemengebietId = csharp.Id, Frage = "Was bedeutet Kapselung?", Antwort = "Interne Daten werden verborgen und nur über definierte Methoden/Properties zugänglich gemacht." },
            new Karteikarte { ThemengebietId = csharp.Id, Frage = "Was ist der Unterschied zwischen einem Werttyp und einem Referenztyp?", Antwort = "Werttypen (z.B. int, struct) enthalten den Wert direkt, Referenztypen (z.B. class) verweisen auf ein Objekt im Heap." },
            new Karteikarte { ThemengebietId = datenbanken.Id, Frage = "Was ist ein Primärschlüssel?", Antwort = "Ein Attribut (oder eine Kombination), das jeden Datensatz einer Tabelle eindeutig identifiziert." },
            new Karteikarte { ThemengebietId = datenbanken.Id, Frage = "Was ist ein Fremdschlüssel?", Antwort = "Ein Attribut, das auf den Primärschlüssel einer anderen Tabelle verweist und so eine Beziehung herstellt." },
            new Karteikarte { ThemengebietId = datenbanken.Id, Frage = "Was besagt die 1. Normalform?", Antwort = "Alle Attribute enthalten nur atomare (unteilbare) Werte." }
        );

        db.SaveChanges();
    }

    private static void SeedLernpfad(ApplicationDbContext db)
    {
        if (db.Lernpfade.Any())
        {
            return;
        }

        var csharp = db.Themengebiete.FirstOrDefault(t => t.Name == "C# Grundlagen");
        if (csharp is null)
        {
            return;
        }

        var karten = db.Karteikarten.Where(k => k.ThemengebietId == csharp.Id).OrderBy(k => k.Id).ToList();
        var fragen = db.Fragen.Where(f => f.ThemengebietId == csharp.Id).OrderBy(f => f.Id).ToList();

        var lernpfad = new Lernpfad
        {
            Titel = "Einstieg in C# und OOP",
            Beschreibung = "Erst die Grundbegriffe mit Karteikarten, dann Verständnisfragen.",
            ThemengebietId = csharp.Id
        };

        var position = 1;
        foreach (var karte in karten)
        {
            lernpfad.Elemente.Add(new LernpfadElement { Typ = ElementTyp.Karteikarte, KarteikarteId = karte.Id, Reihenfolge = position++ });
        }

        foreach (var frage in fragen)
        {
            lernpfad.Elemente.Add(new LernpfadElement { Typ = ElementTyp.Frage, FrageId = frage.Id, Reihenfolge = position++ });
        }

        db.Lernpfade.Add(lernpfad);
        db.SaveChanges();
    }
}
