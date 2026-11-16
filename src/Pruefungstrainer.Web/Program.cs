using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC aktivieren: Controller + Views (statt Razor Pages)
builder.Services.AddControllersWithViews();

// 2. SQLite-DbContext registrieren (Dependency Injection).
//    Die Migrationen liegen im Infrastructure-Projekt, nicht im Web-Projekt.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Pruefungstrainer.Infrastructure")));

var app = builder.Build();

// 3. Beim Start: Datenbank auf den neuesten Migrationsstand bringen und Beispieldaten einfügen.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// 4. Middleware-Pipeline (Reihenfolge ist wichtig!)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 5. Standard-Route: /{Controller}/{Action}/{id?} – Startseite ist der FragenController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Fragen}/{action=Index}/{id?}");

app.Run();
