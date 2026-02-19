using Azure.Extensions.AspNetCore.DataProtection.Blobs;
using Azure.Extensions.AspNetCore.DataProtection.Keys;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using PartsUnlimited.Hubs;
using PartsUnlimited.Models;
using PartsUnlimited.ProductSearch;
using PartsUnlimited.Recommendations;
using PartsUnlimited.Utils;
using System.Globalization;

// Force en-US culture so ToString("C") always renders $ regardless of container locale
var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Azure Key Vault (production)
// ---------------------------------------------------------------------------
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = builder.Configuration["Azure:KeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }
}

// ---------------------------------------------------------------------------
// Entra ID authentication (Microsoft.Identity.Web)
// Falls back to cookie-only auth in Development when AzureAd is not configured,
// so the app can be run locally without an Entra ID app registration.
// ---------------------------------------------------------------------------
// Register LayoutDataFilter for global MVC filter (populates ViewBag.Categories etc.)
builder.Services.AddScoped<LayoutDataFilter>();

var azureAdClientId = builder.Configuration["AzureAd:ClientId"];
var isEntraIdConfigured = !string.IsNullOrWhiteSpace(azureAdClientId)
    && !azureAdClientId.StartsWith('<');

if (isEntraIdConfigured)
{
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    builder.Services
        .AddControllersWithViews(options => options.Filters.AddService<LayoutDataFilter>())
        .AddMicrosoftIdentityUI();
}
else
{
    // Dev-only fallback: cookie auth so [Authorize] redirects to /Account/Login
    // instead of crashing with an OIDC misconfiguration error.
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/AccessDenied";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

    builder.Services.AddControllersWithViews(options => options.Filters.AddService<LayoutDataFilter>());
}
builder.Services.AddAuthorization(options =>
{
    // In development with placeholder AzureAd config, skip the global auth requirement
    // so public pages (Home, Store, Search) are browsable without Entra ID.
    // In production, every unauthenticated request is redirected to sign-in.
    if (!builder.Environment.IsDevelopment())
    {
        options.FallbackPolicy = options.DefaultPolicy;
    }
});

// ---------------------------------------------------------------------------
// Database — EF Core 8 with Azure SQL (Managed Identity in production)
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<PartsUnlimitedContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnectionString"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IPartsUnlimitedContext>(sp =>
    sp.GetRequiredService<PartsUnlimitedContext>());

// ---------------------------------------------------------------------------
// Application services (replaces Unity IoC)
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IProductSearch, StringContainsProductSearch>();
builder.Services.AddScoped<IShippingTaxCalculator, DefaultShippingTaxCalculator>();
builder.Services.AddScoped<ITelemetryProvider, TelemetryProvider>();
builder.Services.AddScoped<IOrdersQuery, OrdersQuery>();
builder.Services.AddScoped<IRaincheckQuery, RaincheckQuery>();

// Choose recommendation engine based on config
if (!string.IsNullOrEmpty(builder.Configuration["MachineLearning:AccountKey"]))
    builder.Services.AddScoped<IRecommendationEngine,
        AzureMLFrequentlyBoughtTogetherRecommendationEngine>();
else
    builder.Services.AddScoped<IRecommendationEngine, NaiveRecommendationEngine>();

// HttpClient for Azure ML
builder.Services.AddHttpClient<AzureMLFrequentlyBoughtTogetherRecommendationEngine>();

// ---------------------------------------------------------------------------
// Caching
// ---------------------------------------------------------------------------
builder.Services.AddMemoryCache();

// AddRazorPages provides the /MicrosoftIdentity/Account/* pages
// (or is a no-op in the dev-cookie-fallback path where those pages aren't needed)
builder.Services.AddRazorPages();

// ---------------------------------------------------------------------------
// SignalR
// ---------------------------------------------------------------------------
builder.Services.AddSignalR();

// ---------------------------------------------------------------------------
// Application Insights
// ---------------------------------------------------------------------------
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);

// Http context accessor for ShoppingCart cookie access
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------------------------
// Data Protection — shared key ring so all pods can decrypt OIDC state cookies
// Requires: Azure Blob Storage (persist keys) + Key Vault RSA key (wrap keys)
// Falls back to in-memory (single-pod / local dev) when config is absent.
// ---------------------------------------------------------------------------
var dpBlobUri    = builder.Configuration["DataProtection:BlobUri"];
var dpKvKeyId    = builder.Configuration["DataProtection:KeyVaultKeyId"];
var dpBuilder    = builder.Services.AddDataProtection().SetApplicationName("PartsUnlimited");
if (!string.IsNullOrEmpty(dpBlobUri) && !string.IsNullOrEmpty(dpKvKeyId))
{
    dpBuilder
        .PersistKeysToAzureBlobStorage(new Uri(dpBlobUri), new DefaultAzureCredential())
        .ProtectKeysWithAzureKeyVault(new Uri(dpKvKeyId), new DefaultAzureCredential());
}

// ---------------------------------------------------------------------------
// Build & configure middleware pipeline
// ---------------------------------------------------------------------------
var app = builder.Build();

// Run EF Core migrations and seed on startup (all environments)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PartsUnlimitedContext>();
    // Apply any pending migrations (creates schema on first run, applies deltas on upgrades)
    await db.Database.MigrateAsync();
    await PartsUnlimitedDbInitializer.SeedAsync(db);
}

var isBehindIngress = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("K8S_INGRESS"));

// When running behind NGINX ingress, trust its X-Forwarded-Proto / X-Forwarded-For headers
// so OIDC redirect URIs are constructed with the correct scheme (https://).
if (isBehindIngress)
{
    var forwardedOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    // Trust all internal cluster IPs — NGINX runs in a known-safe AKS pod network
    forwardedOptions.KnownNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedOptions);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    if (!isBehindIngress)
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
}
app.UseStaticFiles();
app.UseRouting();

// Authentication & authorisation MUST come after UseRouting
app.UseAuthentication();
app.UseAuthorization();

// SignalR hub
app.MapHub<AnnouncementHub>("/hubs/announcement");

// Razor Pages (provides /MicrosoftIdentity/Account/SignIn, SignOut, etc.)
app.MapRazorPages();

// Area routes (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
