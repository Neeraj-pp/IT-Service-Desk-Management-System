
using Capstone.API.Configuration;
using Capstone.API.Middleware;
using Capstone.API.Services;
using HelpDesk.DAL.Model;
using HelpDesk.DAL.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<AuditLogService>();
// JWT configuration
var jwtSection = builder.Configuration.GetSection(
    JwtOptions.SectionName
);

builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing."
    );

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
    string.IsNullOrWhiteSpace(jwtOptions.Key) ||
    Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException(
        "JWT configuration is incomplete. Configure Jwt:Issuer, Jwt:Audience and a Jwt:Key of at least 32 bytes."
    );
}

if (jwtOptions.AccessTokenMinutes is < 5 or > 120)
{
    throw new InvalidOperationException(
        "Jwt:AccessTokenMinutes must be between 5 and 120."
    );
}

// Authentication configuration
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Key)
                ),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>
>();

// Database configuration
var databaseProvider = builder.Configuration[
    "Database:Provider"
];

string? databasePath = null;

if (string.Equals(
    databaseProvider,
    "SQLite",
    StringComparison.OrdinalIgnoreCase))
{
    var databaseFolder = Path.Combine(
        builder.Environment.ContentRootPath,
        "..",
        "Capstone.DAL",
        "Database"
    );

    Directory.CreateDirectory(databaseFolder);

    databasePath = Path.GetFullPath(
        Path.Combine(
            databaseFolder,
            "ServiceDeskDB.db"
        )
    );



    Console.WriteLine("=================================");
    Console.WriteLine("DATABASE VERIFICATION");
    Console.WriteLine($"Database path: {databasePath}");
    Console.WriteLine($"Database exists: {File.Exists(databasePath)}");
    Console.WriteLine("=================================");
}
else if (!string.Equals(
    databaseProvider,
    "SqlServer",
    StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        $"Unsupported database provider: {databaseProvider}"
    );
}

builder.Services.AddDbContext<HelpDeskDbContext>(options =>
{
    if (string.Equals(
        databaseProvider,
        "SQLite",
        StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(
            $"Data Source={databasePath}"
        );
    }
    else
    {
        var connectionString =
            builder.Configuration.GetConnectionString(
                "DefaultConnection"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "SQL Server connection string is not configured."
            );
        }

        options.UseSqlServer(
    connectionString,
    sqlServerOptions =>
    {
        sqlServerOptions.MigrationsAssembly(
            "Capstone.DAL.SqlServerMigrations"
        );
    }
);
    }
});

// Repository
builder.Services.AddScoped<IRepository, Repository>();

// CORS

// CORS configuration
var frontendOrigin =
    builder.Configuration["Frontend:AllowedOrigin"];

if (string.IsNullOrWhiteSpace(frontendOrigin))
{
    throw new InvalidOperationException(
        "Frontend:AllowedOrigin is not configured."
    );
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(frontendOrigin.Trim())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger JWT configuration
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT access token."
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
        }
    );
    // Load XML documentation generated during build
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath = Path.Combine(
        AppContext.BaseDirectory,
        xmlFile
    );

    options.IncludeXmlComments(xmlPath);
});


var app = builder.Build();

Console.WriteLine("=================================");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("=================================");


// Create the initial Admin account in production.
if (app.Environment.IsProduction())
{
    await AdminSeeder.SeedAsync(
        app.Services,
        app.Configuration
    );
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("ReactPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }