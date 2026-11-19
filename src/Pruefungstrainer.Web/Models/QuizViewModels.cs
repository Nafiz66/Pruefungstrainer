using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Web.Models;

/// <summary>
/// Wird in der Session gespeichert, solange ein Quiz läuft. So weiß der Server beim Abgeben,
/// welche Fragen wirklich gestellt wurden – unabhängig davon, was das Formular schickt.
/// </summary>
public class QuizSitzung
{
    public const string SessionSchluessel = "LaufendesQuiz";

    public List<int> FrageIds { get; set; } = new();

    public int? LernpfadId { get; set; }

    public string Titel { get; set; } = string.Empty;

    /// <summary>Nur bei der Prüfungssimulation gesetzt: Zeitpunkt (UTC), an dem die Zeit abläuft.</summary>
    public DateTime? EndetUmUtc { get; set; }
}

/// <summary>Daten für die Seite, auf der die Fragen beantwortet werden.</summary>
public class QuizDurchfuehrenViewModel
{
    public string Titel { get; set; } = string.Empty;

    public List<Frage> Fragen { get; set; } = new();

    public DateTime? EndetUmUtc { get; set; }
}

/// <summary>Eine Antwort, so wie sie aus dem Formular gebunden wird (Antworten[0].FrageId usw.).</summary>
public class QuizAntwortEingabe
{
    public int FrageId { get; set; }

    public List<int> GewaehlteOptionIds { get; set; } = new();

    public string? Freitext { get; set; }
}
