using MassTransit;
using System.Text;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RouteGuard.Platform.FleetRouteManagement.Application.CommandServices;
using RouteGuard.Platform.FleetRouteManagement.Application.Internal.CommandServices;
using RouteGuard.Platform.FleetRouteManagement.Application.Internal.QueryServices;
using RouteGuard.Platform.FleetRouteManagement.Application.QueryServices;
using RouteGuard.Platform.FleetRouteManagement.Domain.Repositories;
using RouteGuard.Platform.FleetRouteManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.FleetRouteManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.IdentityAccessManagement.Application.Acl;
using RouteGuard.Platform.IdentityAccessManagement.Application.CommandServices;
using RouteGuard.Platform.IdentityAccessManagement.Application.Internal.CommandServices;
using RouteGuard.Platform.IdentityAccessManagement.Application.Internal.OutboundServices;
using RouteGuard.Platform.IdentityAccessManagement.Application.Internal.QueryServices;
using RouteGuard.Platform.IdentityAccessManagement.Application.QueryServices;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Repositories;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Hashing.BCrypt.Services;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Tokens.Jwt.Configuration;
using RouteGuard.Platform.IdentityAccessManagement.Infrastructure.Tokens.Jwt.Services;
using RouteGuard.Platform.IdentityAccessManagement.Interfaces.Acl;
using RouteGuard.Platform.NotificationsCommunication.Application.CommandServices;
using RouteGuard.Platform.NotificationsCommunication.Application.Internal.CommandServices;
using RouteGuard.Platform.NotificationsCommunication.Application.Internal.QueryServices;
using RouteGuard.Platform.NotificationsCommunication.Application.QueryServices;
using RouteGuard.Platform.NotificationsCommunication.Domain.Repositories;
using RouteGuard.Platform.NotificationsCommunication.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.NotificationsCommunication.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.Shared.Resources;
using RouteGuard.Platform.Shared.Resources.Errors;
using RouteGuard.Platform.StakeholderAssetManagement.Application.CommandServices;
using RouteGuard.Platform.StakeholderAssetManagement.Application.Internal.CommandServices;
using RouteGuard.Platform.StakeholderAssetManagement.Application.Internal.QueryServices;
using RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.CommandServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.Internal.CommandServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.Internal.QueryServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Application.QueryServices;
using RouteGuard.Platform.SubscriptionPlanManagement.Domain.Repositories;
using RouteGuard.Platform.SubscriptionPlanManagement.Infraestructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.SubscriptionPlanManagement.Infraestructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.TripExecutionMonitoring.Application.CommandServices;
using RouteGuard.Platform.TripExecutionMonitoring.Application.Internal;
using RouteGuard.Platform.TripExecutionMonitoring.Application.Internal.CommandServices;
using RouteGuard.Platform.TripExecutionMonitoring.Application.Internal.QueryServices;
using RouteGuard.Platform.TripExecutionMonitoring.Application.QueryServices;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Repositories;
using RouteGuard.Platform.TripExecutionMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Seeding;
using RouteGuard.Platform.TripExecutionMonitoring.Infrastructure.Repositories;

// Keep DateTime values as 'timestamp without time zone' (as with MySQL) so a DateTime with an unspecified Kind
// (e.g. one coming from a JSON request) is not rejected by Npgsql.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Routing & Controllers
// ---------------------------------------------------------------------------
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddDataAnnotationsLocalization();

builder.Services.AddProblemDetails();

// ---------------------------------------------------------------------------
// CORS
// ---------------------------------------------------------------------------
// Only the origins listed in Cors:AllowedOrigins may call the API from a browser. Development with no list
// stays open for local tools; any other environment with no list allows no cross-origin access at all
// (the mobile apps do not use CORS).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
        else if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// ---------------------------------------------------------------------------
// Database (PostgreSQL via EF Core / Npgsql)
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionStringTemplate))
        throw new InvalidOperationException("Database connection string is not set in the configuration.");

    var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);

    options.UseNpgsql(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// ---------------------------------------------------------------------------
// Authentication (JWT Bearer) — issued by the Iam bounded context
// ---------------------------------------------------------------------------
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

var tokenSecret = builder.Configuration["TokenSettings:Secret"];
if (string.IsNullOrWhiteSpace(tokenSecret))
    throw new InvalidOperationException("TokenSettings:Secret is not set in the configuration.");

// The secret may be an environment placeholder (e.g. %JWT_SECRET%). It must resolve to a real value of at
// least 32 characters; otherwise the API refuses to start instead of signing tokens with a weak key.
tokenSecret = Environment.ExpandEnvironmentVariables(tokenSecret);
if (tokenSecret.StartsWith('%') || tokenSecret.Length < 32)
    throw new InvalidOperationException(
        "TokenSettings:Secret must be at least 32 characters. Set it with an environment variable " +
        "(JWT_SECRET) or user-secrets; it must not be committed to the repository.");
builder.Services.PostConfigure<TokenSettings>(settings => settings.Secret = tokenSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
// Every endpoint requires an authenticated user unless it is marked [AllowAnonymous]; role rules are declared
// per action with [Authorize(Roles = AppRoles.…)].
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddHttpContextAccessor();

// Behind Azure App Service (or any reverse proxy) TLS ends at the proxy: trust X-Forwarded-For / -Proto so the
// API sees the real client address and scheme.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Liveness probe used by the hosting platform (Azure App Service health check).
builder.Services.AddHealthChecks();
builder.Services.AddScoped<CallerContext>();

// ---------------------------------------------------------------------------
// Localization
// ---------------------------------------------------------------------------
builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();

// Custom RFC 7807 problem details factory.
builder.Services.AddSingleton<ProblemDetailsFactory>();

// ---------------------------------------------------------------------------
// Swagger / OpenAPI
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "RouteGuard-Platform",
            Version = "v1",
            Description = "RouteGuard Platform API",
            License = new OpenApiLicense
            {
                Name = "Apache 2.0",
                Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
            }
        });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
    options.EnableAnnotations();
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

// ---------------------------------------------------------------------------
// Dependency Injection per Bounded Context
// ---------------------------------------------------------------------------

// Shared
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Stakeholder bounded context
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IStudentGroupRepository, StudentGroupRepository>();
builder.Services.AddScoped<IParentCommandService, ParentCommandService>();
builder.Services.AddScoped<IParentQueryService, ParentQueryService>();
builder.Services.AddScoped<IDriverCommandService, DriverCommandService>();
builder.Services.AddScoped<IDriverQueryService, DriverQueryService>();
builder.Services.AddScoped<IStudentGroupCommandService, StudentGroupCommandService>();
builder.Services.AddScoped<IStudentGroupQueryService, StudentGroupQueryService>();

// Trip bounded context
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<ITripCommandService, TripCommandService>();
builder.Services.AddScoped<ITripQueryService, TripQueryService>();

// Fleet bounded context
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteCommandService, RouteCommandService>();
builder.Services.AddScoped<IRouteQueryService, RouteQueryService>();

// Subscription bounded context
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IPlanCommandService, PlanCommandService>();
builder.Services.AddScoped<IPlanQueryService, PlanQueryService>();
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();

// Notifications bounded context
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationCommandService, NotificationCommandService>();
builder.Services.AddScoped<INotificationQueryService, NotificationQueryService>();

// Iam bounded context
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IOrganizationCommandService, OrganizationCommandService>();
builder.Services.AddScoped<IOrganizationQueryService, OrganizationQueryService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

// ---------------------------------------------------------------------------
// Mediator (Cortex) — command pipeline behaviors and event handling
// ---------------------------------------------------------------------------
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddCortexMediator([typeof(Program)]);

// ---------------------------------------------------------------------------
// MassTransit & RabbitMQ
// ---------------------------------------------------------------------------
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RouteGuard.Platform.NotificationsCommunication.Application.Internal.EventHandlers.OfflineSyncCompletedEventConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var rmqUrl = builder.Configuration["RabbitMQ:Url"];
        if (!string.IsNullOrEmpty(rmqUrl))
        {
            var uri = Environment.ExpandEnvironmentVariables(rmqUrl);
            cfg.Host(new Uri(uri));
        }
        else
        {
            var host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
            var username = builder.Configuration["RabbitMQ:Username"] ?? "guest";
            var password = builder.Configuration["RabbitMQ:Password"] ?? "guest";

            cfg.Host(host, "/", h =>
            {
                h.Username(username);
                h.Password(password);
            });
        }

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Apply pending migrations on startup (safe to call even when the schema is up to date).
// Wrapped so a database connectivity/credential problem does not prevent the API (and Swagger)
// from starting: the app boots, surfaces a clear log entry, and database-backed endpoints fail
// individually until the connection is fixed.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var startupLogger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        startupLogger.LogInformation("Database migrations applied successfully.");

        // Seed demo data on an empty database, in dependency order across bounded contexts:
        // identity/org first, then stakeholders, then plans/subscription, routes, trips and
        // finally notifications that reference the seeded parent and trip.
        // Demo data (users with well known passwords, demo routes and trips) is only created in Development, or
        // when Seed:Enabled is true. Reference data (plan catalog, notification templates) is always ensured.
        var seedDemoData = app.Configuration.GetValue<bool?>("Seed:Enabled") ?? app.Environment.IsDevelopment();
        if (seedDemoData)
        {
            var hashingService = services.GetRequiredService<IHashingService>();
            await IamSeeder.SeedAsync(context, hashingService);
            await StakeholderSeeder.SeedAsync(context);
            await SubscriptionSeeder.SeedAsync(context);
            await FleetSeeder.SeedAsync(context);
            await TripSeeder.SeedAsync(context);
            await NotificationSeeder.SeedAsync(context);
            startupLogger.LogInformation("Demo seed data ensured.");
        }
        else
        {
            await SubscriptionSeeder.SeedPlansAsync(context);
            await NotificationSeeder.SeedTemplatesAsync(context);
            startupLogger.LogInformation("Demo seed data skipped (reference data ensured).");
        }
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex,
            "Database migration failed at startup. The API will still start, but database-backed " +
            "endpoints will fail until the connection string in appsettings is corrected.");
    }
}

// ---------------------------------------------------------------------------
// HTTP request pipeline
// ---------------------------------------------------------------------------
app.UseForwardedHeaders();
app.UseGlobalExceptionHandler();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseCors("DefaultCorsPolicy");
// HTTPS redirection only outside Development so local http://localhost:8080 calls
// (Swagger and the Vue frontend) are not redirected to the self-signed https port.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
