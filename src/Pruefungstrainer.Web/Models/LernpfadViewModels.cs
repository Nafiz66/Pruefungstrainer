using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pruefungstrainer.Core.Entities;

namespace Pruefungstrainer.Web.Models;

/// <summary>Formulardaten für "Lernpfad anlegen/bearbeiten".</summary>
public class LernpfadFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte einen Titel eingeben.")]
    [MaxLength(150)]
    public string Titel { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Beschreibung { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Bitte ein Themengebiet wählen.")]
    [Display(Name = "Themengebiet")]
    public int ThemengebietId { get; set; }

    [ValidateNever]
    public List<SelectListItem> ThemengebietAuswahl { get; set; } = new();

    /// <summary>Beim Bearbeiten: Hat der Lernpfad schon Elemente? Dann darf das Themengebiet nicht mehr wechseln.</summary>
    [ValidateNever]
    public bool HatElemente { get; set; }

    public static LernpfadFormViewModel AusEntitaet(Lernpfad lernpfad) => new()
    {
        Id = lernpfad.Id,
        Titel = lernpfad.Titel,
        Beschreibung = lernpfad.Beschreibung,
        ThemengebietId = lernpfad.ThemengebietId,
        HatElemente = lernpfad.Elemente.Count > 0
    };

    public Lernpfad ZuEntitaet() => new()
    {
        Id = Id,
        Titel = Titel,
        Beschreibung = Beschreibung,
        ThemengebietId = ThemengebietId
    };
}

/// <summary>Seite "Elemente verwalten": der Lernpfad und die Inhalte, die man noch hinzufügen kann.</summary>
public class LernpfadElementeViewModel
{
    public Lernpfad Lernpfad { get; set; } = null!;

    public List<SelectListItem> VerfuegbareKarteikarten { get; set; } = new();

    public List<SelectListItem> VerfuegbareFragen { get; set; } = new();
}
