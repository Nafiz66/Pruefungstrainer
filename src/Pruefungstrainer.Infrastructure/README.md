# Infrastructure-Layer – EF Core / SQLite

Namespace: `Pruefungstrainer.Infrastructure.Data`
(Konfigurationen unter `Pruefungstrainer.Infrastructure.Data.Configurations`)

## Projektstruktur

```
Infrastructure/
  Data/
    ApplicationDbContext.cs
    Configurations/
      ApplicationUserConfiguration.cs
      ThemengebietConfiguration.cs
      LernpfadConfiguration.cs
      LernpfadElementConfiguration.cs
      KarteikarteConfiguration.cs
      FrageConfiguration.cs
      AntwortoptionConfiguration.cs
      KarteikartenFortschrittConfiguration.cs
      PruefungsergebnisConfiguration.cs
      PruefungsergebnisFrageConfiguration.cs
  appsettings.Beispiel.json
```

Jede Entität hat genau eine eigene `IEntityTypeConfiguration<T>`-Klasse. `ApplicationDbContext.OnModelCreating`
lädt sie automatisch per `ApplyConfigurationsFromAssembly`, sodass die Datei kurz und übersichtlich bleibt,
egal wie viele Entitäten später noch dazukommen.

## 1. NuGet-Pakete

Im **Infrastructure-Projekt** (Class Library, referenziert den Core-Layer):

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

Im **Web-Projekt** (Startup-Projekt, das die Migrationen ausführt):

```bash
dotnet add package Microsoft.EntityFrameworkCore.Design
```

`Microsoft.EntityFrameworkCore.Design` wird nur zur Design-Zeit für `dotnet ef`-Befehle benötigt
und muss deshalb im Startup-Projekt (Web) liegen, nicht zwingend im Infrastructure-Projekt.

## 2. appsettings.json (Web-Projekt)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=pruefungstrainer.db"
  }
}
```

Für Produktion/Tests kann hier später z.B. ein absoluter Pfad oder ein Pfad in
`AppData` hinterlegt werden – SQLite legt die Datei einfach als lokale Datei an.

## 3. Program.cs (Web-Projekt)

```csharp
using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ... vorhandene Services (Controllers, Razor Pages, etc.) ...

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' wurde nicht gefunden.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Optional für die Entwicklung: Datenbank beim Start automatisch anlegen/aktualisieren.
// Für ein IHK-Projekt reicht das meist aus; produktiv würde man Migrationen
// eher über ein separates Deployment-Skript ausführen.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ... restliche Pipeline (app.MapControllers(), app.Run(), ...) ...
```

## 4. Migrationen erstellen und anwenden

Ausgeführt im Projektstamm (dort, wo die `.sln` liegt), da Core/Infrastructure/Web
vermutlich als separate Projekte in einer Solution liegen:

```bash
# Erste Migration auf Basis der aktuellen Entitäten erzeugen
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Web

# Datenbank (SQLite-Datei) gemäß Migration anlegen/aktualisieren
dotnet ef database update --project Infrastructure --startup-project Web
```

`--project` gibt an, wo der `DbContext` liegt (Infrastructure), `--startup-project` gibt an,
welches Projekt beim Ausführen der Befehle gestartet wird (Web), da dort die
Konfiguration (Connection String, `DbContext`-Registrierung) zusammenläuft.

## 5. Kurze Begründung der wichtigsten Entscheidungen

- **Enums als String speichern** (`HasConversion<string>()`): Macht die SQLite-Datenbank beim
  manuellen Prüfen (z.B. mit DB Browser for SQLite) sofort lesbar – wichtig für die
  Nachvollziehbarkeit im Rahmen deiner IHK-Dokumentation.
- **`DeleteBehavior.Restrict`** bei Stammdaten (Themengebiet → Lernpfad/Karteikarte/Frage,
  Karteikarte/Frage → LernpfadElement, Frage → PruefungsergebnisFrage): verhindert, dass durch
  ein versehentliches Löschen still und heimlich Prüfungsergebnisse oder Lerninhalte verschwinden.
- **`DeleteBehavior.Cascade`** bei reinen "Detail"-/Verknüpfungsdaten (Elemente eines Lernpfads,
  Antwortoptionen einer Frage, Fortschritts- und Ergebnisdetails eines Benutzers): diese Daten
  ergeben ohne ihren "Besitzer" keinen Sinn und sollen automatisch mitgelöscht werden.
- **Check-Constraint bei `LernpfadElement`**: stellt auf Datenbankebene sicher, dass passend
  zum `Typ` immer genau `KarteikarteId` **oder** `FrageId` gesetzt ist – so kann selbst bei
  einem Bug in der Anwendungslogik kein inkonsistenter Datensatz entstehen.
