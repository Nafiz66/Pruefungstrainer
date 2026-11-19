using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Interfaces;
using Pruefungstrainer.Infrastructure.Data;
using Pruefungstrainer.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC aktivieren: Controller + Views (statt Razor Pages)
builder.Services.AddControllersWithViews();

// 2. SQLite-DbContext registrieren (Dependency Injection).
//    Die Migrationen liegen im Infrastructure-Projekt, nicht im Web-Projekt.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Pruefungstrainer.Infrastructure")));

// 3. Eigene Services registrieren: Wer IThemengebietService verlangt, bekommt einen ThemengebietService.
//    Scoped = ein Objekt pro HTTP-Anfrage (passend zum DbContext, der ebenfalls Scoped ist).
builder.Services.AddScoped<IThemengebietService, ThemengebietService>();

var app = builder.Build();

// 4. Beim Start: Datenbank auf den neuesten Migrationsstand bringen und Beispieldaten einfügen.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// 5. Middleware-Pipeline (Reihenfolge ist wichtig!)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 6. Standard-Route: /{Controller}/{Action}/{id?} – Startseite ist der FragenController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Fragen}/{action=Index}/{id?}");

app.Run();
