using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Infrastructure.Data;

/// <summary>
/// Füllt eine leere Datenbank mit ein paar Beispieldaten, damit man direkt
/// nach dem ersten Start etwas auf den Seiten sieht.
/// Liegt in der Infrastructure, weil es Datenzugriff ist – nicht Aufgabe des Web-Projekts.
/// </summary>
public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        // Nur einmal befüllen: Sind schon Themengebiete da, passiert nichts.
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
}
