using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RifaTech.API.Context;
using RifaTech.API.Endpoints;
using RifaTech.API.Extensions;
using RifaTech.API.Middleware;
using RifaTech.API.Services;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. DATABASE
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.MigrationsAssembly("RifaTech.API")));

// ============================================================
// 2. IDENTITY
// ============================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ============================================================
// 3. AUTHENTICATION (JWT)
// ============================================================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ============================================================
// 4. AUTHORIZATION
// ============================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// ============================================================
// 5. CORS (Restricted — configure allowed origins)
// ============================================================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://localhost:7145", "http://localhost:5000", "http://localhost:7230", "https://localhost:7230"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================================
// 6. SWAGGER / OPENAPI
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RifaTech API",
        Version = "v1",
        Description = "API para gerenciamento de rifas online com integração Mercado Pago."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ============================================================
// 7. CACHING
// ============================================================
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// ============================================================
// 8. AUTOMAPPER
// ============================================================
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// ============================================================
// 9. APPLICATION SERVICES (single registration)
// ============================================================
builder.Services.AddApplicationServices();

// ============================================================
// 10. JSON SERIALIZATION
// ============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Configure JSON for Minimal APIs (endpoints using Results.Ok / TypedResults)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// ============================================================
// 11. HEALTH CHECKS
// ============================================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// ============================================================
// 12. RATE LIMITING (protects public endpoints from abuse)
// ============================================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("PublicEndpoints", limiterOptions =>
    {
        limiterOptions.PermitLimit = 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    options.AddFixedWindowLimiter("PaymentEndpoints", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });
});

// ============================================================
// BUILD APPLICATION
// ============================================================
var app = builder.Build();

// ============================================================
// DATABASE MIGRATION (auto-apply pending migrations on startup)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying pending database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations.");
        throw;
    }
}

// ============================================================
// ROLE INITIALIZATION (after Build, using the real service provider)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = ["Admin", "User", "Mestre"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// ============================================================
// DATABASE SEEDING (example data for development)
// ============================================================
await DatabaseSeeder.SeedAsync(app.Services, app.Logger);

// ============================================================
// MIDDLEWARE PIPELINE (order matters!)
// ============================================================

// 1. Global exception handler (first in pipeline to catch everything)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        logger.LogError(exception, "Unhandled exception at {Path}", context.Request.Path);

        context.Response.StatusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{context.Response.StatusCode}",
            title = exception switch
            {
                KeyNotFoundException => "Resource not found",
                ArgumentException => "Invalid request",
                UnauthorizedAccessException => "Forbidden",
                _ => "Internal server error"
            },
            status = context.Response.StatusCode,
            detail = app.Environment.IsDevelopment() ? exception?.Message : "An error occurred processing your request.",
            traceId = context.TraceIdentifier
        });
    });
});

// 2. Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// 3. Response Caching
app.UseResponseCaching();

// 4. Cache control headers for public endpoints
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/rifas") ||
        context.Request.Path.StartsWithSegments("/rifa"))
    {
        context.Response.Headers.Append("Cache-Control", "public,max-age=60");
    }

    await next();
});

// 5. CORS
app.UseCors("DefaultPolicy");

// 6. Rate Limiting
app.UseRateLimiter();

// 7. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 8. Custom admin auth middleware
app.UseAdminAuth();

// ============================================================
// HEALTH CHECK ENDPOINT
// ============================================================
app.MapHealthChecks("/health").AllowAnonymous();

// ============================================================
// REGISTER ENDPOINTS
// ============================================================
app.RegisterAccountEndpoints();
app.RegisterRifaEndpoints();
app.RegisterClienteEndpoints();
app.RegisterTicketEndpoints();
app.RegisterPaymentEndpoints();
app.RegisterDrawEndpoints();
app.RegisterExtraNumberEndpoints();
app.RegisterUnpaidRifaEndpoints();
app.RegisterRifaMetricsEndpoints();
app.RegisterCompraRapidaEndpoints();
app.RegisterWebhookEndpoints();
app.RegisterAdminDashboardEndpoints();
app.RegisterDrawManagementEndpoints();
app.RegisterTicketSearchEndpoints();

app.Run();