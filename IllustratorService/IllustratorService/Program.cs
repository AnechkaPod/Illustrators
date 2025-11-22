using IllustratorService.Data;
using IllustratorService.Middleweare;
using IllustratorService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;  // ← ADDED THIS
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ═══════════════════════════════════════════════════════════════════
// UPDATED: Configure Swagger with JWT Authentication Support
// This will show the "Authorize" button in Swagger UI
// ═══════════════════════════════════════════════════════════════════
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Illustrators API",
        Version = "v1",
        Description = "API for managing illustrator profiles"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddDbContext<IllustratorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIllustratorService, IllustratorService.Services.IllustratorService>();

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

//builder.Services.AddHealthChecks()
//    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseMiddleware<JwtAuthenticationMiddleware>();

app.MapControllers();

//app.MapHealthChecks("/health");

//app.MapGet("/", () => new
//{
//    service = "Illustrator Service",
//    version = "1.0.0",
//    status = "running"
//});

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("❌❌❌ STARTUP ERROR ❌❌❌");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
    }
    throw;
}