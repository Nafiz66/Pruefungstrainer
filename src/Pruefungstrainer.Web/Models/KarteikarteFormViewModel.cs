using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Web.Models;

/// <summary>Formulardaten für "Karteikarte anlegen/bearbeiten".</summary>
public class KarteikarteFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte die Vorderseite (Frage) ausfüllen.")]
    [MaxLength(500)]
    [Display(Name = "Vorderseite (Frage)")]
    public string Frage { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte die Rückseite (Antwort) ausfüllen.")]
    [MaxLength(1000)]
    [Display(Name = "Rückseite (Antwort)")]
    public string Antwort { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Bitte ein Themengebiet wählen.")]
    [Display(Name = "Themengebiet")]
    public int ThemengebietId { get; set; }

    [ValidateNever]
    public List<SelectListItem> ThemengebietAuswahl { get; set; } = new();

    public static KarteikarteFormViewModel AusEntitaet(Karteikarte karte) => new()
    {
        Id = karte.Id,
        Frage = karte.Frage,
        Antwort = karte.Antwort,
        ThemengebietId = karte.ThemengebietId
    };

    public Karteikarte ZuEntitaet() => new()
    {
        Id = Id,
        Frage = Frage,
        Antwort = Antwort,
        ThemengebietId = ThemengebietId
    };
}
