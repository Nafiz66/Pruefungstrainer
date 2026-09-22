# Core-Layer – Entitäten (Prüfungstrainingsplattform)

Namespace: `PruefungstrainingPlattform.Core.Entities`

## Übersicht der Entitäten

| Entität | Zweck |
|---|---|
| `ApplicationUser` | Benutzer mit Rolle `Lehrkraft` oder `Teilnehmer` |
| `Themengebiet` | Fachliches Thema (z.B. "Datenbanken – AP1") |
| `Lernpfad` | Strukturierter Lernpfad, gehört zu einem Themengebiet |
| `LernpfadElement` | Ordnet Karteikarten/Fragen in die Reihenfolge eines Lernpfads ein |
| `Karteikarte` | Frage-Antwort-Karte, gehört zu einem Themengebiet |
| `Frage` | Quiz-/Prüfungsfrage (Multiple Choice oder Freitext) |
| `Antwortoption` | Antwortmöglichkeit einer Multiple-Choice-Frage |
| `KarteikartenFortschritt` | Lernstatus eines Teilnehmers zu einer Karteikarte (n:m mit Zusatzdaten) |
| `Pruefungsergebnis` | Gesamtergebnis eines Quiz-/Prüfungsdurchlaufs |
| `PruefungsergebnisFrage` | Einzelbewertung je Frage innerhalb eines Prüfungsergebnisses |

## Beziehungen (Kardinalitäten)

- `Themengebiet` 1—n `Lernpfad`
- `Themengebiet` 1—n `Karteikarte`
- `Themengebiet` 1—n `Frage`
- `Lernpfad` 1—n `LernpfadElement` (jedes Element zeigt **entweder** auf `Karteikarte` **oder** `Frage`, gesteuert über `ElementTyp`)
- `Frage` 1—n `Antwortoption`
- `ApplicationUser` 1—n `KarteikartenFortschritt` n—1 `Karteikarte` (klassische n:m-Verknüpfungstabelle mit Zusatzattributen wie `Status`, `WiederholungenAnzahl`)
- `ApplicationUser` 1—n `Pruefungsergebnis` (optional n—1 `Lernpfad`)
- `Pruefungsergebnis` 1—n `PruefungsergebnisFrage` n—1 `Frage`

## Hinweise zur EF-Core-Nutzung

- Alle Primärschlüssel folgen der Konvention `Id` (int, wird von EF Core automatisch als Primärschlüssel erkannt).
- Fremdschlüssel sind über `[ForeignKey]` explizit annotiert und folgen zusätzlich der Namenskonvention `<Navigation>Id`, sodass EF Core sie auch ohne Fluent-API-Konfiguration korrekt erkennt.
- Optionale Fremdschlüssel (`int?`) bilden optionale Beziehungen ab (z.B. `Pruefungsergebnis.LernpfadId`).
- Für `LernpfadElement` empfiehlt sich zusätzlich eine `CHECK`-Constraint bzw. Validierung auf Anwendungsebene, dass **genau eine** der beiden Referenzen (`KarteikarteId` oder `FrageId`) passend zu `Typ` gesetzt ist – dies lässt sich mit Data Annotations allein nicht vollständig abbilden.
- Für die SQLite-Persistenz sollten in der `DbContext`-Konfiguration (Infrastructure-Layer) u.a. folgende Punkte ergänzt werden:
  - `OnDelete(DeleteBehavior.Restrict)` an sinnvollen Stellen, um unbeabsichtigte Kaskadenlöschungen zu vermeiden (SQLite unterstützt kaskadierendes Löschen, aber bei Prüfungsergebnissen ist "Restrict" oft sinnvoller).
  - Eindeutiger Index auf `ApplicationUser.Email`.
  - Eindeutiger, zusammengesetzter Index auf `(LernpfadId, Reihenfolge)` in `LernpfadElement`.
  - Eindeutiger, zusammengesetzter Index auf `(ApplicationUserId, KarteikarteId)` in `KarteikartenFortschritt`, um Duplikate zu verhindern.

Diese Konfigurationen gehören in eine `OnModelCreating`-Methode im Infrastructure-Layer (`DbContext`), nicht in die Entitäten selbst, um den Core-Layer frei von EF-Core-spezifischer Konfigurationslogik zu halten.
