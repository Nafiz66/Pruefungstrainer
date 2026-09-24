using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
using Pruefungstrainer.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Auf Controller & Views umstellen (statt Razor Pages)
builder.Services.AddControllersWithViews();

// 2. SQLite DbContext konfigurieren
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Pruefungstrainer.Infrastructure")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    SeedData(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 3. Das Standard-Routing für unsere Controller aktivieren (Startpunkt: FragenController)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Fragen}/{action=Index}/{id?}");

app.Run();

static void SeedData(ApplicationDbContext db)
{
    if (db.Themengebiete.Any())
    {
        return;
    }

    var themen = new[]
    {
        new Themengebiet { Id = 1, Name = "C# Grundlagen", Kategorie = "AP1", Beschreibung = "Grundlagen der C#-Programmierung und OOP." },
        new Themengebiet { Id = 2, Name = "Datenbanken", Kategorie = "AP2", Beschreibung = "Relationale Datenbanken, SQL und Entity Framework." }
    };

    db.Themengebiete.AddRange(themen);

    var fragen = new[]
    {
        new Frage { Id = 1, Fragetext = "Welche Programmiersprache wird für ASP.NET Core häufig verwendet?", Typ = Fragetyp.MultipleChoice, Punkte = 2, ThemengebietId = 1 },
        new Frage { Id = 2, Fragetext = "Was beschreibt eine SQL-Tabelle am besten?", Typ = Fragetyp.MultipleChoice, Punkte = 3, ThemengebietId = 2 },
        new Frage { Id = 3, Fragetext = "Nenne ein Prinzip der objektorientierten Programmierung.", Typ = Fragetyp.Freitext, Punkte = 5, ThemengebietId = 1, MusterloesungFreitext = "Kapselung, Vererbung oder Polymorphie." }
    };

    db.Fragen.AddRange(fragen);

    db.Antwortoptionen.AddRange(
        new Antwortoption { Id = 1, Text = "C#", IstKorrekt = true, FrageId = 1 },
        new Antwortoption { Id = 2, Text = "Java", IstKorrekt = false, FrageId = 1 },
        new Antwortoption { Id = 3, Text = "Python", IstKorrekt = false, FrageId = 1 },
        new Antwortoption { Id = 4, Text = "Eine Sammlung von Datensätzen mit gleicher Struktur", IstKorrekt = true, FrageId = 2 },
        new Antwortoption { Id = 5, Text = "Ein Browser-Plugin", IstKorrekt = false, FrageId = 2 },
        new Antwortoption { Id = 6, Text = "Ein CSS-Stil", IstKorrekt = false, FrageId = 2 }
    );

    db.SaveChanges();
}