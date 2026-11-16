namespace Pruefungstrainer.Web.Models;

/// <summary>
/// Daten für die Fehlerseite (Views/Shared/Error.cshtml).
/// Die RequestId hilft, einen Fehler im Log wiederzufinden.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
