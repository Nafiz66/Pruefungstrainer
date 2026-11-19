namespace Pruefungstrainer.Core.Models;

/// <summary>
/// Die Antwort eines Teilnehmers auf eine einzelne Quizfrage, so wie sie aus dem Formular kommt.
/// Bei Multiple Choice sind die angekreuzten Antwortoptionen gefüllt, bei Freitext der Text.
/// </summary>
public class QuizAntwort
{
    public int FrageId { get; set; }

    public List<int> GewaehlteOptionIds { get; set; } = new();

    public string? Freitext { get; set; }
}
