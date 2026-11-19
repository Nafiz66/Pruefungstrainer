using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Web.Models;

/// <summary>Übersicht im Lernbereich: Themengebiete mit ihren Inhalten und alle Lernpfade.</summary>
public class LernenIndexViewModel
{
    public List<ThemengebietKachel> Themengebiete { get; set; } = new();

    public List<Lernpfad> Lernpfade { get; set; } = new();
}

public class ThemengebietKachel
{
    public Themengebiet Themengebiet { get; set; } = null!;

    public int AnzahlKarteikarten { get; set; }

    public int AnzahlFragen { get; set; }

    public int GelernteKarteikarten { get; set; }
}

/// <summary>Karteikarten-Lernmodus für ein Themengebiet.</summary>
public class KarteikartenLernenViewModel
{
    public Themengebiet Themengebiet { get; set; } = null!;

    /// <summary>Die aktuelle Karte oder null, wenn alle gelernt sind.</summary>
    public Karteikarte? Karte { get; set; }

    public int Gelernt { get; set; }

    public int Gesamt { get; set; }
}

/// <summary>Ein einzelner Schritt beim Bearbeiten eines Lernpfads.</summary>
public class LernpfadSchrittViewModel
{
    public Lernpfad Lernpfad { get; set; } = null!;

    public LernpfadElement Element { get; set; } = null!;

    /// <summary>Aktuelle Position, beginnend bei 1.</summary>
    public int Schritt { get; set; }

    public int AnzahlSchritte { get; set; }

    public bool HatFragen { get; set; }

    public bool IstLetzterSchritt => Schritt == AnzahlSchritte;
}
