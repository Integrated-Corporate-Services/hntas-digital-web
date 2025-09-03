using GovUk.OneLogin.AspNetCore;
using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Client;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Routing;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using HNTAS.Web.UI.Workflows.Services;
using HNTAS.Web.UI.Workflows.Validation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .SetApplicationName("HNTAS.Web.UI");

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

builder.Services.AddHttpContextAccessor();

Console.WriteLine("*********************in UI**************");
Console.WriteLine("Environment: " + builder.Environment.EnvironmentName);
Console.WriteLine("from env variable: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

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
        new OrgRegisteredAddressJsonConverter(),
        new InitialUserRegistrationRequestJsonConverter(),
        new UserRoleJsonConverter(),
        new UserResponseJsonConverter(),
        new OrganisationJsonConverter(),
        new HeatNetworkJsonConverter(),
        new EnumItemResponseJsonConverter(),
        new InvitationJsonConverter(),
        new ContributorRoleJsonConverter(),
        new UserDetailsResponseJsonConverter(),
        new OrganisationResponseJsonConverter(),
        new HeatNetworkResponseJsonConverter(),
        new RegisteredAddressJsonConverter(),
        new ManagedUserResponseJsonConverter(),
        new InvitedUserResponseJsonConverter(),
        new HnRoleMappingJsonConverter()
    }
});
builder.Services.AddSingleton<JsonSerializerOptionsProvider>();

builder.Services.AddSingleton<UsersApiEvents>();
builder.Services.AddHttpClient<IUsersApi, UsersApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddSingleton<HeatNetworksApiEvents>();
builder.Services.AddHttpClient<IHeatNetworksApi, HeatNetworksApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<InvitationsApiEvents>();
builder.Services.AddHttpClient<InvitationsApi, InvitationsApi>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddScoped<ISessionHelper, SessionHelper>();

builder.Services.AddScoped<IWorkflowManager, WorkflowManager>();
// Since it's a generic filter, you can register a specific type for each workflow
builder.Services.AddScoped<WorkflowValidationFilter<AddNewContributorWorkflowModel, ContributorWorkflowStep>>();
builder.Services.AddScoped<WorkflowValidationFilter<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>>();
builder.Services.AddScoped<IRedirectResolver<AddNewContributorWorkflowModel, ContributorWorkflowStep>, NewContributorRedirectResolver>();
builder.Services.AddScoped<IRedirectResolver<AddExistingContributorWorkflowModel, ExistingContributorWorkflowStep>, ExistingContributorRedirectResolver>();

builder.Services.AddScoped<EnsureSessionForOrganisationFlowOnGetAttribute>();
builder.Services.AddScoped<EnsureSessionForOrganisationFlowOnPostAttribute>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();

builder.Services.AddScoped<IHeatNetworkService, HeatNetworkService>();

builder.Services.AddHttpClient<ICompaniesHouseService, CompaniesHouseService>();

builder.Services.AddScoped<IInvitationTokenService, InvitationTokenService>();


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
        // Assign individual event handlers
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            var invitedEmail = context.HttpContext.Session.GetString(SessionKeys.InvitedTokenEmail)?.Trim('"');
            var invitationId = context.HttpContext.Session.GetString(SessionKeys.InvitationId)?.Trim('"');
            var inviterUserId = context.HttpContext.Session.GetString(SessionKeys.InvitedInviterUserId)?.Trim('"');
            var inviterOrgId = context.HttpContext.Session.GetString(SessionKeys.InvitedInviterUserOrgId)?.Trim('"');

            if (!string.IsNullOrWhiteSpace(invitedEmail) &&
                !string.IsNullOrWhiteSpace(invitationId) &&
                !string.IsNullOrWhiteSpace(inviterUserId) &&
                !string.IsNullOrWhiteSpace(inviterOrgId))
            {
                var customState = $"{invitedEmail}|{invitationId}|{inviterUserId}|{inviterOrgId}";
                context.ProtocolMessage.State = customState;
            }

            return Task.CompletedTask;
        };

        // You can assign other events similarly
        options.Events.OnTokenValidated = context =>
        {
            var state = context.ProtocolMessage.State;
            var parts = state?.Split('|');

            if (parts?.Length == 4)
            {
                var invitedEmail = parts[0];
                var invitationId = parts[1];
                var inviterUserId = parts[2];
                var inviterOrgId = parts[3];

                var identity = (ClaimsIdentity)context.Principal.Identity!;
                identity.AddClaim(new Claim("hntas.invitedEmail", invitedEmail));
                identity.AddClaim(new Claim("hntas.invitationId", invitationId));
                identity.AddClaim(new Claim("hntas.inviterUserId", inviterUserId));
                identity.AddClaim(new Claim("hntas.inviterOrgId", inviterOrgId));
            }

            return Task.CompletedTask;
        };
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

app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


//This is to check if the application is in maintenance mode

var maintenanceMode = Environment.GetEnvironmentVariable("MAINTENANCE_MODE");
if (!string.IsNullOrEmpty(maintenanceMode) && maintenanceMode.Equals("true", StringComparison.OrdinalIgnoreCase))
{
    app.Use(async (context, next) =>
    {
        // Allow access to the maintenance page and static assets
        var path = context.Request.Path.Value;
        if (path != null && (path.Equals("/maintenance.html", StringComparison.OrdinalIgnoreCase) ||
                             path.StartsWith("/assets") ||
                             path.StartsWith("/css") ||
                             path.StartsWith("/js")))
        {
            await next();
        }
        else
        {
            context.Response.Redirect("/maintenance.html");
        }
    });
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/home/start-page"));

app.MapControllerRoute(
    name: "default",
    pattern: "[controller]/[action]/{id?}",
    defaults: new { controller = "Home", action = "StartPage" });

app.Run();
