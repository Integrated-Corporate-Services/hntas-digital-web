using GovUk.OneLogin.AspNetCore;
using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Client;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Routing;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


// Configure RouteOptions for lowercase URLs
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true; // Optional: for query string parameters too
});

// Register the parameter transformer globally for controllers/actions
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new SlugifiedRouteConvention());
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

// Dynamically load appsettings.{ENVIRONMENT}.json if it exists
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true);

var coreApiBaseUrl = builder.Configuration.GetValue<string>("ApiClients:CoreApiBaseUrl");

// It's crucial to validate that the configuration value was actually found
if (string.IsNullOrEmpty(coreApiBaseUrl))
    throw new InvalidOperationException("The 'ApiClients:CoreApiBaseUrl' is not configured in appsettings.json. Please ensure it exists and has a value.");

// Register CompaniesHouseService with HttpClientFactory
builder.Services.AddSingleton(new JsonSerializerOptions
{

    PropertyNameCaseInsensitive = true, // Common setting for JSON deserialization
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Common setting for JSON serialization
    Converters = {
        new UserJsonConverter(),
        new OrgDetailsJsonConverter(),
        new OrgRegisteredAddressJsonConverter(),
        new InitialUserRegistrationRequestJsonConverter(),
        new UserRoleJsonConverter(),
        new UserResponseJsonConverter()
    }
});
builder.Services.AddSingleton<JsonSerializerOptionsProvider>();
builder.Services.AddSingleton<UsersApiEvents>();
builder.Services.AddHttpClient<IUsersApi, UsersApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddHttpClient<ICompaniesHouseService, CompaniesHouseService>();

//Configure onelogin settings
builder.Services.AddAuthentication(defaultScheme: OneLoginDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOneLogin(options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Environment = OneLoginEnvironments.Integration;
        options.ClientId = Environment.GetEnvironmentVariable("ONELOGIN_CLIENT_ID");
        options.CallbackPath = "/onelogin-callback";
        options.SignedOutCallbackPath = "/onelogin-logout-callback";
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("phone");
        // options.Scope.Add("profile"); // If your service needs name, birthdate, etc.

        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(Environment.GetEnvironmentVariable("ONELOGIN_PRIVATE_KEY").AsSpan().ToString().Replace("\\n", "\n"));
            options.ClientAuthenticationCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(true)), // Fix: Ensure RsaSecurityKey is resolved
                SecurityAlgorithms.RsaSha256);
        }

        options.VectorsOfTrust = ["Cl"];
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddHttpClient<AddressLookupService>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


try
{
    var govukAssetPath = Path.Combine(app.Environment.ContentRootPath, "node_modules/govuk-frontend/dist/govuk/assets");
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(govukAssetPath),
        RequestPath = "/assets"
    });
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Unable to map govuk_frontend assets.");
}

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/heat-network-eligibility/running-ahn"));

app.MapControllerRoute(
    name: "default",
    pattern: "[controller]/[action]/{id?}",
    defaults: new { controller = "HeatNetworkEligibility", action = "RunningAHN" });

app.Run();