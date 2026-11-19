using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Web.Models;

/// <summary>
/// Formulardaten für "Frage anlegen/bearbeiten".
/// Ein eigenes ViewModel statt der Entität, weil das Formular zusätzlich die Dropdown-Liste
/// und eine feste Anzahl Antwortfelder braucht – und nur erlaubte Felder angenommen werden sollen.
/// </summary>
public class FrageFormViewModel : IValidatableObject
{
    /// <summary>So viele Antwortfelder zeigt das Formular. Leere Felder werden ignoriert.</summary>
    public const int AnzahlAntwortfelder = 4;

    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte einen Fragetext eingeben.")]
    [MaxLength(1000)]
    public string Fragetext { get; set; } = string.Empty;

    [Display(Name = "Fragetyp")]
    public Fragetyp Typ { get; set; } = Fragetyp.MultipleChoice;

    [Range(1, 100, ErrorMessage = "Die Punkte müssen zwischen 1 und 100 liegen.")]
    public int Punkte { get; set; } = 1;

    [MaxLength(1000)]
    [Display(Name = "Musterlösung")]
    public string? MusterloesungFreitext { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Bitte ein Themengebiet wählen.")]
    [Display(Name = "Themengebiet")]
    public int ThemengebietId { get; set; }

    public List<AntwortoptionEingabe> Antwortoptionen { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> ThemengebietAuswahl { get; set; } = new();

    /// <summary>Fachliche Regeln, die über einzelne Felder hinausgehen.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Typ == Fragetyp.MultipleChoice)
        {
            var ausgefuellt = Antwortoptionen.Where(a => !string.IsNullOrWhiteSpace(a.Text)).ToList();

            if (ausgefuellt.Count < 2)
            {
                yield return new ValidationResult("Eine Multiple-Choice-Frage braucht mindestens zwei Antwortoptionen.");
            }

            if (!ausgefuellt.Any(a => a.IstKorrekt))
            {
                yield return new ValidationResult("Mindestens eine Antwortoption muss als richtig markiert sein.");
            }
        }
        else if (string.IsNullOrWhiteSpace(MusterloesungFreitext))
        {
            yield return new ValidationResult(
                "Für eine Freitextfrage wird eine Musterlösung benötigt.",
                new[] { nameof(MusterloesungFreitext) });
        }
    }

    /// <summary>Füllt auf die feste Anzahl Antwortfelder auf, damit das Formular immer gleich aussieht.</summary>
    public void FuelleAntwortfelderAuf()
    {
        while (Antwortoptionen.Count < AnzahlAntwortfelder)
        {
            Antwortoptionen.Add(new AntwortoptionEingabe());
        }
    }

    public static FrageFormViewModel AusEntitaet(Frage frage)
    {
        var viewModel = new FrageFormViewModel
        {
            Id = frage.Id,
            Fragetext = frage.Fragetext,
            Typ = frage.Typ,
            Punkte = frage.Punkte,
            MusterloesungFreitext = frage.MusterloesungFreitext,
            ThemengebietId = frage.ThemengebietId,
            Antwortoptionen = frage.Antwortoptionen
                .OrderBy(a => a.Id)
                .Select(a => new AntwortoptionEingabe { Text = a.Text, IstKorrekt = a.IstKorrekt })
                .ToList()
        };
        viewModel.FuelleAntwortfelderAuf();
        return viewModel;
    }

    public Frage ZuEntitaet()
    {
        return new Frage
        {
            Id = Id,
            Fragetext = Fragetext,
            Typ = Typ,
            Punkte = Punkte,
            MusterloesungFreitext = MusterloesungFreitext,
            ThemengebietId = ThemengebietId,
            Antwortoptionen = Antwortoptionen
                .Where(a => !string.IsNullOrWhiteSpace(a.Text))
                .Select(a => new Antwortoption { Text = a.Text!.Trim(), IstKorrekt = a.IstKorrekt })
                .ToList()
        };
    }
}

/// <summary>Ein Antwortfeld im Frage-Formular.</summary>
public class AntwortoptionEingabe
{
    [MaxLength(500)]
    public string? Text { get; set; }

    public bool IstKorrekt { get; set; }
}
