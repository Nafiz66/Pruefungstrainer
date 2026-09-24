namespace Pruefungstrainer.Core.Entities;

/// <summary>
/// Rollen der Benutzer innerhalb der Plattform.
/// </summary>
public enum UserRole
{
    /// <summary>Erstellt und pflegt Inhalte (Themengebiete, Lernpfade, Fragen, Karteikarten).</summary>
    Lehrkraft = 0,

    /// <summary>Bearbeitet Lerninhalte und Prüfungssimulationen.</summary>
    Teilnehmer = 1
}

/// <summary>
/// Typ einer Prüfungsfrage.
/// </summary>
public enum Fragetyp
{
    /// <summary>Frage mit vorgegebenen Antwortoptionen (<see cref="Antwortoption"/>).</summary>
    MultipleChoice = 0,

    /// <summary>Frage mit freier Texteingabe, die manuell oder gegen eine Musterlösung bewertet wird.</summary>
    Freitext = 1
}

/// <summary>
/// Lernstatus einer Karteikarte für einen bestimmten Teilnehmer.
/// </summary>
public enum KarteikartenStatus
{
    /// <summary>Karteikarte wurde noch nicht bearbeitet.</summary>
    Neu = 0,

    /// <summary>Karteikarte wird aktuell wiederholt/gelernt.</summary>
    InBearbeitung = 1,

    /// <summary>Karteikarte gilt als gelernt.</summary>
    Gelernt = 2
}

/// <summary>
/// Gibt an, auf welchen Inhaltstyp ein <see cref="LernpfadElement"/> verweist.
/// </summary>
public enum ElementTyp
{
    Karteikarte = 0,
    Frage = 1
}
