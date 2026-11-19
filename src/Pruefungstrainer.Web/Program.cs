using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pruefungstrainer.Core.Entities;
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

// 3. Eigene Services registrieren: Wer das Interface verlangt, bekommt die Implementierung.
//    Scoped = ein Objekt pro HTTP-Anfrage (passend zum DbContext, der ebenfalls Scoped ist).
builder.Services.AddScoped<IThemengebietService, ThemengebietService>();
builder.Services.AddScoped<IFrageService, FrageService>();
builder.Services.AddScoped<IKarteikarteService, KarteikarteService>();
builder.Services.AddScoped<IKarteikartenLernService, KarteikartenLernService>();
builder.Services.AddScoped<ILernpfadService, LernpfadService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IBenutzerService, BenutzerService>();

// Passwort-Hashing aus ASP.NET Core Identity (PBKDF2 mit Salt). Zustandslos, daher Singleton.
builder.Services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

// 4. Anmeldung per Cookie: Nach dem Login speichert ein verschlüsseltes Cookie,
//    wer der Benutzer ist und welche Rolle er hat (Claims).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/ZugriffVerweigert";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true; // JavaScript kann das Cookie nicht lesen (Schutz vor XSS-Diebstahl)
    });

builder.Services.AddAuthorization();

// 5. Session: merkt sich serverseitig, welche Fragen in einem laufenden Quiz gestellt wurden.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 6. Beim Start: Datenbank auf den neuesten Migrationsstand bringen und Beispieldaten einfügen.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
    db.Database.Migrate();
    DbSeeder.Seed(db, passwordHasher);
}

// 7. Middleware-Pipeline (Reihenfolge ist wichtig!)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Wer bist du?  (liest das Login-Cookie)
app.UseAuthorization();  // Darfst du das? (prüft [Authorize] und Rollen)

app.UseSession();

// 8. Standard-Route: /{Controller}/{Action}/{id?} – Startseite ist Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
