using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Data.Seeders;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services;
using Sifp_Vue.Server.Services.Contracts;

// ---------------------------------------------------------------------------
// Perintah CLI: migrate / migrate:fresh / seed (lihat "npm run migrate" dkk
// di package.json root). Diambil sebelum CreateBuilder karena argumen posisi
// tanpa awalan "-" bikin CommandLineConfigurationProvider bawaan ASP.NET Core
// melempar FormatException.
// ---------------------------------------------------------------------------
string? cliCommand = null;
if (args.Length > 0 && !args[0].StartsWith('-'))
{
    cliCommand = args[0];
    args = args[1..];
}

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Konfigurasi
// ---------------------------------------------------------------------------

builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));
builder.Services.Configure<ImportOptions>(builder.Configuration.GetSection(ImportOptions.SectionName));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection(AzureAdOptions.SectionName));

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
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<ITotpService, TotpService>();
builder.Services.AddSingleton<IMfaChallengeTokenService, MfaChallengeTokenService>();

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

    // Memungkinkan endpoint /api yang kini berpagar [Authorize] dicoba langsung dari Swagger UI.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token dari POST /api/auth/login, mis. \"eyJhbGci...\" (tanpa awalan \"Bearer \")."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ---------------------------------------------------------------------------
// Autentikasi: cookie untuk /admin (Razor), token bearer JWT untuk /api (klien
// Vue) lewat AuthController (/api/auth/login, /me), dan "Sign in with Microsoft"
// (Microsoft Entra ID / OpenID Connect) lewat MicrosoftAuthController.
// Skema default tetap cookie supaya area /admin tidak berubah; endpoint /api
// menegaskan skemanya sendiri lewat [Authorize(AuthenticationSchemes = ...)].
// ---------------------------------------------------------------------------

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSigningKey = jwtSection["SigningKey"];
var azureAd = builder.Configuration.GetSection(AzureAdOptions.SectionName).Get<AzureAdOptions>() ?? new AzureAdOptions();

var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = string.IsNullOrWhiteSpace(jwtSigningKey)
                ? null
                : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

// "Sign in with Microsoft" hanya diaktifkan bila App registration sudah diisi
// (AzureAd:TenantId + ClientId). Bila belum, skema tidak didaftarkan dan tombol
// SSO membalas pesan "belum dikonfigurasi" alih-alih membuat server gagal start.
if (azureAd.IsConfigured)
{
    authBuilder
        // Cookie sementara penampung hasil OIDC selama handshake redirect.
        .AddCookie(AzureAdOptions.OidcCookieScheme, options =>
        {
            options.Cookie.Name = "Sifp.Oidc";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        })
        .AddOpenIdConnect(AzureAdOptions.OidcScheme, options =>
        {
            options.Authority = azureAd.Authority;
            options.ClientId = azureAd.ClientId;
            options.ClientSecret = azureAd.ClientSecret;
            options.CallbackPath = azureAd.CallbackPath;
            options.SignInScheme = AzureAdOptions.OidcCookieScheme;

            // Authorization Code flow dengan response_mode=query supaya cookie
            // korelasi (SameSite=Lax) tetap terkirim saat Microsoft me-redirect
            // balik lewat navigasi GET — bekerja di dev (http) maupun prod (https).
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.ResponseMode = OpenIdConnectResponseMode.Query;
            options.UsePkce = true;
            options.SaveTokens = false;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");

            options.GetClaimsFromUserInfoEndpoint = true;
            options.CallbackPath = azureAd.CallbackPath;

            options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.NonceCookie.SameSite = SameSiteMode.Lax;
            options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            options.TokenValidationParameters.NameClaimType = "preferred_username";
        });
}

authBuilder
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
// Perintah CLI eksplisit: jalankan lalu keluar tanpa menyalakan Kestrel.
// ---------------------------------------------------------------------------

if (cliCommand is not null)
{
    if (!await TryRunCliCommandAsync(app, cliCommand))
    {
        Console.Error.WriteLine(
            $"Perintah '{cliCommand}' tidak dikenal. Yang tersedia: migrate, migrate:fresh, seed.");
        Environment.ExitCode = 1;
    }

    return;
}

// ---------------------------------------------------------------------------
// Migration + seeding otomatis. Hanya di luar Development supaya deploy
// (systemctl start, tanpa langkah "npm run migrate" terpisah) tetap
// ter-migrate otomatis seperti sebelumnya. Saat dev, `dotnet run` polos
// sekarang cuma menyalakan backend — pakai `npm run migrate` / `npm run seed`
// dari root secara eksplisit (lihat package.json).
// ---------------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
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

// Setara `php artisan migrate` / `migrate:fresh` / `db:seed`, dipanggil lewat
// `npm run migrate` dkk. `migrate:fresh` hanya reset skema (tidak seed) supaya
// simetris dengan perintah `seed` terpisah — data awal tetap terisi otomatis
// begitu aplikasi dijalankan normal (lihat DatabaseSeeder.RunAsync di atas).
static async Task<bool> TryRunCliCommandAsync(WebApplication app, string command)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<SifpDbContext>();

    switch (command)
    {
        case "migrate":
        {
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count == 0)
            {
                logger.LogInformation("Database sudah pada versi migration terbaru.");
            }
            else
            {
                logger.LogInformation("Menjalankan {Count} migration: {Names}", pending.Count, string.Join(", ", pending));
                await db.Database.MigrateAsync();
            }
            return true;
        }

        case "migrate:fresh":
        {
            logger.LogWarning("Menghapus seluruh database dan menjalankan ulang semua migration dari awal...");
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
            logger.LogInformation("Database sudah di-reset. Jalankan 'npm run seed' atau start aplikasi untuk mengisi data awal.");
            return true;
        }

        case "seed":
        {
            var seeders = services.GetServices<IDataSeeder>();
            foreach (var seeder in seeders.OrderBy(s => s.Order))
            {
                try
                {
                    await seeder.SeedAsync();
                    logger.LogInformation("Seeder {Name} selesai.", seeder.Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Seeder {Name} gagal", seeder.Name);
                }
            }
            return true;
        }

        default:
            return false;
    }
}
