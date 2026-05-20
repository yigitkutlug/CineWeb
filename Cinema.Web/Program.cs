using Cinema.Infrastructure.Data;
using Cinema.Web.Infrastructure.Email;
using Cinema.Web.Infrastructure.HostedServices;
using Cinema.Application.Movies;
using Cinema.Application.Auth;
using Cinema.Application.Showtimes;
using Cinema.Application.Tickets;
using Cinema.Application.Users;
using Cinema.Application.Profiles;
using Cinema.Application.AdminMovies;
using Cinema.Application.Actors;
using Cinema.Application.Dashboard;
using Cinema.Application.Reviews;
using Cinema.Application.Badges;
using Cinema.Infrastructure.Auth;
using Cinema.Infrastructure.Movies;
using Cinema.Infrastructure.Tickets;
using Cinema.Infrastructure.Showtimes;
using Cinema.Infrastructure.Reviews;
using Cinema.Infrastructure.Badges;
using Cinema.Infrastructure.Profiles;
using Cinema.Infrastructure.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

// Uygulamanin giris noktasi: servis kayitlari, middleware ve route'lar burada kurulur.
var builder = WebApplication.CreateBuilder(args);

// Identity UI (Login/Register vb.) Razor Pages ile geldigi icin gerekli.
builder.Services.AddRazorPages();

// MVC controller + view destegi.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cinema API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Format: \"Bearer {token}\""
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<ShowtimeExpirationHostedService>();

// EF Core + PostgreSQL baglantisi.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
        npgsql => npgsql.MigrationsAssembly("Cinema.Infrastructure")));

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("JWT ayarlari eksik. appsettings.json icindeki Jwt bolumunu kontrol et.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Application servisleri (query/command) burada DI'a eklenir.
builder.Services.AddScoped<IMovieQueryService, MovieQueryService>();
builder.Services.AddScoped<IMovieCatalogService, MovieCatalogService>();
builder.Services.AddScoped<ISeatSelectionService, SeatSelectionService>();
builder.Services.AddScoped<ITicketPurchaseService, TicketPurchaseService>();
builder.Services.AddScoped<ITicketNotificationService, EmailTicketNotificationService>();
builder.Services.AddScoped<IShowtimeQueryService, ShowtimeQueryService>();
builder.Services.AddScoped<IShowtimeAdminService, ShowtimeAdminService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAdminMovieService, AdminMovieService>();
builder.Services.AddScoped<IActorAdminService, ActorAdminService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IShowtimeExpirationService, ShowtimeExpirationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// Identity kullanici sistemi + rol destegi.
builder.Services.AddDefaultIdentity<Microsoft.AspNetCore.Identity.IdentityUser>(options =>
{
    // Gelistirme asamasinda e-posta onayi zorunlu degil.
    options.SignIn.RequireConfirmedAccount = false;
})


.AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();



var app = builder.Build();

// Uygulama acilirken roller ve SuperAdmin hesabi seed edilir.
await SeedData.InitializeAsync(app.Services, app.Configuration);


// wwwroot altindaki statik dosyalari sunar.
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Route eslestirme altyapisi.
app.UseRouting();

// Kullanici kimligini (cookie/token) okur.
app.UseAuthentication();
// [Authorize] kurallarini uygular.
app.UseAuthorization();

// Area route (Admin panel gibi alanlar icin).
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Varsayilan site route'u.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity Razor Pages endpoint'leri.
app.MapRazorPages();

// Uygulamayi baslatir.
app.Run();
