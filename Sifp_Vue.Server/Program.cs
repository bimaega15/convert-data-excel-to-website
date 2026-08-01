using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Data.Seeders;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services;
using Sifp_Vue.Server.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Konfigurasi
// ---------------------------------------------------------------------------

builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));
builder.Services.Configure<ImportOptions>(builder.Configuration.GetSection(ImportOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("SifpDatabase");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'SifpDatabase' belum diatur. Isi di appsettings.Development.json, " +
        "user-secrets, atau environment variable ConnectionStrings__SifpDatabase.");
}

builder.Services.AddDbContext<SifpDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.MigrationsAssembly(typeof(SifpDbContext).Assembly.FullName);
        // Retry bawaan menangani kegagalan sesaat pada SQL Azure / jaringan lambat.
        sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    }));

// ---------------------------------------------------------------------------
// Dependency injection
// ---------------------------------------------------------------------------

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IUserClaimsFactory, UserClaimsFactory>();

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IObservationRepository, ObservationRepository>();
builder.Services.AddScoped<ISifQuestionRepository, SifQuestionRepository>();
builder.Services.AddScoped<IErrorTrapRepository, ErrorTrapRepository>();
builder.Services.AddScoped<IHpToolRepository, HpToolRepository>();
builder.Services.AddScoped<IDriftConditionRepository, DriftConditionRepository>();
builder.Services.AddScoped<ILatentConditionRepository, LatentConditionRepository>();
builder.Services.AddScoped<ICcvcLibraryRepository, CcvcLibraryRepository>();
builder.Services.AddScoped<IInitiativeRepository, InitiativeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IWorksheetRepository, WorksheetRepository>();
builder.Services.AddScoped<IImportBatchRepository, ImportBatchRepository>();

// Services
builder.Services.AddScoped<IObservationService, ObservationService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IInitiativeService, InitiativeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorksheetService, WorksheetService>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();

// Seeders
builder.Services.AddScoped<IDataSeeder, IdentitySeeder>();
builder.Services.AddScoped<IDataSeeder, MasterDataSeeder>();
builder.Services.AddScoped<DatabaseSeeder>();

// ---------------------------------------------------------------------------
// MVC + API
// ---------------------------------------------------------------------------

builder.Services.AddControllersWithViews();

// [ApiController] membalas ModelState tidak valid secara otomatis dengan
// ProblemDetails. Factory ini menggantinya dengan amplop ApiResponse supaya
// seluruh respons /api punya bentuk yang sama, termasuk saat validasi gagal.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            Sifp_Vue.Server.Models.Dtos.ApiResponse<object>.Fail("Data yang dikirim tidak valid.", errors));
    };
});

builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIFP Assurance API",
        Version = "v1",
        Description = "API untuk dashboard SIFP Assurance Regional 4 (klien Vue)."
    });

    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);

    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ---------------------------------------------------------------------------
// Autentikasi: cookie untuk /admin (Razor).
//
// Endpoint /api sengaja terbuka — klien Vue tidak punya halaman login sendiri.
// Pembatasan akses aplikasi direncanakan lewat Windows Authentication di IIS
// perusahaan, sehingga tidak diduplikasi di level aplikasi.
// ---------------------------------------------------------------------------

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "Sifp.Admin.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.Events = new CookieAuthenticationEvents
        {
            // Permintaan XHR/JSON menerima 401 alih-alih halaman login HTML,
            // supaya kode klien tidak salah mengira redirect sebagai keberhasilan.
            OnRedirectToLogin = context =>
            {
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Area /admin hanya untuk role yang ditandai CanAccessAdmin di seeder.
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireRole(RoleNames.Administrator, RoleNames.Verifier);
    });
});

// ---------------------------------------------------------------------------
// CORS untuk dev server Vite
// ---------------------------------------------------------------------------

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Migration + seeding
// ---------------------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.RunAsync();
}

// ---------------------------------------------------------------------------
// Pipeline
// ---------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIFP Assurance API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}
else
{
    app.UseExceptionHandler("/Shared/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowVueClient");
app.UseAuthentication();
app.UseAuthorization();

// Controller ber-atribut lebih dulu: /api/* dan /admin/* memakai [Route].
app.MapControllers();

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Admin}/{action=Index}/{id?}");

// Sisanya diserahkan ke aplikasi Vue (hasil `npm run build` disalin ke wwwroot).
// /api dan /admin sengaja tidak di-fallback supaya rute yang salah tetap 404.
app.MapFallback(async context =>
{
    var path = context.Request.Path.Value ?? string.Empty;

    if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    var indexPath = Path.Combine(app.Environment.WebRootPath ?? string.Empty, "index.html");
    if (File.Exists(indexPath))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(indexPath);
        return;
    }

    // Saat pengembangan, klien Vue dijalankan terpisah lewat `npm run dev`.
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await context.Response.WriteAsync(
        "Build aplikasi Vue belum ada di wwwroot. Jalankan `npm run dev` di sifp_vue.client " +
        "(http://localhost:5173), atau `npm run build` untuk menyalin hasil build ke sini.");
});

app.Run();

static bool IsApiRequest(HttpRequest request) =>
    request.Path.StartsWithSegments("/api") ||
    request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
    request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
