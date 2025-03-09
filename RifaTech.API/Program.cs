using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Add application services
builder.Services.AddApplicationServices();

// Add background services
builder.Services.AddHostedService<PaymentStatusVerificationService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RifaTech API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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
            new string[] {}
        }
    });
});

// Add Memory Cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add Response Caching
builder.Services.AddResponseCaching();
// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Policy for Admin access
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    // Policy for authenticated users (both Admin and regular users)
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add application services
builder.Services.AddApplicationServices();

// Add JSON serialization options to handle circular references
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Initialize roles
await builder.Services.InitializeRoles(builder.Services.BuildServiceProvider());
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString),
        x => x.MigrationsAssembly("RifaTech.API")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();


    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Criar role Admin se não existir
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Criar usuário admin se não existir
        var adminUser = await userManager.FindByEmailAsync("admin@rifatech.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@rifatech.com",
                Email = "admin@rifatech.com",
                Name = "Administrador",
                CPF = "12345678900",
                EhAdmin = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
    }
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Use Response Caching
app.UseResponseCaching();

// Add cache control middleware
app.Use(async (context, next) =>
{
    // Set Cache-Control header for public endpoints
    if (context.Request.Path.StartsWithSegments("/rifas") ||
        context.Request.Path.StartsWithSegments("/rifa"))
    {
        context.Response.Headers.Append("Cache-Control", "public,max-age=60"); // 1 minute
    }

    await next();
});

// Use CORS
app.UseCors("AllowAll");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Use admin auth middleware to identify admin users
app.UseAdminAuth();

// Register endpoints
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