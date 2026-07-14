using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using NisanDavetiye.API.Middleware;
using NisanDavetiye.BLL;
using NisanDavetiye.BLL.Options;
using NisanDavetiye.DAL;
using NisanDavetiye.DAL.Data;
using NisanDavetiye.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=nisandavetiye.db";

// SQLite dosyasının bulunduğu klasörü (ör. Railway Volume /data) başlamadan önce oluştur.
var sqliteDataSource = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString).DataSource;
var sqliteDirectory = Path.GetDirectoryName(sqliteDataSource);
if (!string.IsNullOrWhiteSpace(sqliteDirectory))
    Directory.CreateDirectory(sqliteDirectory);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddDal(connectionString);
builder.Services.AddBll(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("public-forms", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            ResolveClientIp(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 20,
                QueueLimit = 0
            }));
    options.AddPolicy("panel-api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            $"panel:{ResolveClientIp(httpContext)}",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 60,
                QueueLimit = 0
            }));
    options.AddPolicy("panel-access", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            $"panel-access:{ResolveClientIp(httpContext)}",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 30,
                QueueLimit = 0
            }));
});

var galeriUploadRelative = builder.Configuration["GaleriStorage:UploadDirectory"] ?? "uploads/galeri";
var galeriPublicPrefix = builder.Configuration["GaleriStorage:PublicUrlPrefix"] ?? "/uploads/galeri";
var galeriAbsolutePath = Path.IsPathRooted(galeriUploadRelative)
    ? galeriUploadRelative
    : Path.Combine(builder.Environment.ContentRootPath, galeriUploadRelative);

builder.Services.Configure<GaleriStorageOptions>(options =>
{
    options.UploadDirectory = galeriUploadRelative;
    options.PublicUrlPrefix = galeriPublicPrefix;
    options.AbsoluteUploadDirectory = galeriAbsolutePath;
    options.MaxDailyUploadCount = builder.Configuration.GetValue("GaleriStorage:MaxDailyUploadCount", 100);
});

var adminApiKey = builder.Configuration["Admin:ApiKey"] ?? string.Empty;
if (!builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(adminApiKey) || adminApiKey.Length < 32)
    {
        throw new InvalidOperationException(
            "Üretim ortamında Admin:ApiKey en az 32 karakter olmalıdır.");
    }
}
else if (string.IsNullOrWhiteSpace(adminApiKey) || adminApiKey.Length < 32)
{
    Console.WriteLine("UYARI: Admin:ApiKey zayıf veya eksik. Üretimde en az 32 karakter kullanın.");
}

var mediaSigningKey = builder.Configuration["MediaSigning:SigningKey"];
if (string.IsNullOrWhiteSpace(mediaSigningKey))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("MediaSigning:SigningKey yapılandırılmalıdır.");

    mediaSigningKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
    builder.Configuration["MediaSigning:SigningKey"] = mediaSigningKey;
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("UiPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:4173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

var galeriStorage = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<GaleriStorageOptions>>().Value;
Directory.CreateDirectory(galeriStorage.AbsoluteUploadDirectory);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NisanDavetiyeDbContext>();
    var davetiyeRepo = scope.ServiceProvider.GetRequiredService<IDavetiyeRepository>();
    db.Database.Migrate();
    await EnsureRsvpSchemaAsync(db);
    await EnsureDavetUidSchemaAsync(db);
    await EnsurePanelUidSchemaAsync(db);
    await EnsureGaleriOnaySchemaAsync(db);
    await davetiyeRepo.EnsureDavetUidAsync();
    await davetiyeRepo.EnsurePanelUidAsync();
    await DavetiyeDataSeeder.SeedAsync(db);

    var panelUidForLog = await davetiyeRepo.GetPanelUidAsync();
    if (!string.IsNullOrEmpty(panelUidForLog))
    {
        app.Logger.LogInformation(
            "Yönetim paneli: http://localhost:5173/p/{PanelUid}",
            panelUidForLog);
    }
}

app.UseForwardedHeaders();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("UiPolicy");
app.UseRateLimiter();
app.UseSecurity();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    timestamp = DateTimeOffset.UtcNow
}));

// SPA fallback: /api/* istekleri asla index.html'e düşmez; diğer route'lar UI'a yönlenir.
app.MapFallback("/api/{**rest}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();

static string ResolveClientIp(HttpContext httpContext)
{
    var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(forwarded))
        return forwarded.Split(',')[0].Trim();

    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

static async Task EnsureRsvpSchemaAsync(NisanDavetiyeDbContext db)
{
    await using var connection = db.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

    await using var check = connection.CreateCommand();
    check.CommandText = """
        SELECT COUNT(*)
        FROM pragma_table_info('RsvpKayitlari')
        WHERE name = 'AdminListedenGizli'
        """;
    var exists = Convert.ToInt64(await check.ExecuteScalarAsync() ?? 0L) > 0;
    if (exists)
        return;

    await using var alter = connection.CreateCommand();
    alter.CommandText = """
        ALTER TABLE RsvpKayitlari
        ADD COLUMN AdminListedenGizli INTEGER NOT NULL DEFAULT 0
        """;
    await alter.ExecuteNonQueryAsync();
}

static async Task EnsureDavetUidSchemaAsync(NisanDavetiyeDbContext db)
{
    await using var connection = db.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

    await using var check = connection.CreateCommand();
    check.CommandText = """
        SELECT COUNT(*)
        FROM pragma_table_info('DavetiyeAyarlari')
        WHERE name = 'DavetUid'
        """;
    var exists = Convert.ToInt64(await check.ExecuteScalarAsync() ?? 0L) > 0;
    if (exists)
        return;

    await using var alter = connection.CreateCommand();
    alter.CommandText = """
        ALTER TABLE DavetiyeAyarlari
        ADD COLUMN DavetUid TEXT NOT NULL DEFAULT ''
        """;
    await alter.ExecuteNonQueryAsync();
}

static async Task EnsurePanelUidSchemaAsync(NisanDavetiyeDbContext db)
{
    await using var connection = db.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

    await using var check = connection.CreateCommand();
    check.CommandText = """
        SELECT COUNT(*)
        FROM pragma_table_info('DavetiyeAyarlari')
        WHERE name = 'PanelUid'
        """;
    var exists = Convert.ToInt64(await check.ExecuteScalarAsync() ?? 0L) > 0;
    if (exists)
        return;

    await using var alter = connection.CreateCommand();
    alter.CommandText = """
        ALTER TABLE DavetiyeAyarlari
        ADD COLUMN PanelUid TEXT NOT NULL DEFAULT ''
        """;
    await alter.ExecuteNonQueryAsync();
}

static async Task EnsureGaleriOnaySchemaAsync(NisanDavetiyeDbContext db)
{
    await using var connection = db.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

    await using var checkOnay = connection.CreateCommand();
    checkOnay.CommandText = """
        SELECT COUNT(*)
        FROM pragma_table_info('GaleriResimleri')
        WHERE name = 'Onaylandi'
        """;
    var onayExists = Convert.ToInt64(await checkOnay.ExecuteScalarAsync() ?? 0L) > 0;
    if (!onayExists)
    {
        await using var alterOnay = connection.CreateCommand();
        alterOnay.CommandText = """
            ALTER TABLE GaleriResimleri
            ADD COLUMN Onaylandi INTEGER NOT NULL DEFAULT 1
            """;
        await alterOnay.ExecuteNonQueryAsync();
    }

    await using var checkTarih = connection.CreateCommand();
    checkTarih.CommandText = """
        SELECT COUNT(*)
        FROM pragma_table_info('GaleriResimleri')
        WHERE name = 'YuklemeTarihi'
        """;
    var tarihExists = Convert.ToInt64(await checkTarih.ExecuteScalarAsync() ?? 0L) > 0;
    if (!tarihExists)
    {
        await using var alterTarih = connection.CreateCommand();
        alterTarih.CommandText = """
            ALTER TABLE GaleriResimleri
            ADD COLUMN YuklemeTarihi TEXT NOT NULL DEFAULT '1970-01-01T00:00:00'
            """;
        await alterTarih.ExecuteNonQueryAsync();

        await using var backfillTarih = connection.CreateCommand();
        backfillTarih.CommandText = """
            UPDATE GaleriResimleri
            SET YuklemeTarihi = datetime('now')
            WHERE YuklemeTarihi = '1970-01-01T00:00:00'
            """;
        await backfillTarih.ExecuteNonQueryAsync();
    }
}
