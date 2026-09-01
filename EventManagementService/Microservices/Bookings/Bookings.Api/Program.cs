using System.Text; 
using Bookings.Api;
using Bookings.Application; 
using Bookings.Infrastructure; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args); 
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "bookings-service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(configuration["Otlp:Endpoint"]!)))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT-токен."
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", null),
            new List<string>()
        }
    });
}); 
builder.Services.AddDbContext<BookingsDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("BookingsDb"))); 
builder.Services.AddScoped<IBookingRepository, BookingRepository>(); 
builder.Services.AddSingleton<IBookingConfirmedPublisher, KafkaBookingConfirmedPublisher>();
builder.Services.AddScoped<IBookingService, BookingService>(); 
builder.Services.AddHostedService<BookingProcessingService>();
var jwt = builder.Configuration.GetSection("Jwt"); 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x => x.TokenValidationParameters = new() 
                                                                                        { ValidateIssuer = true, ValidIssuer = jwt["Issuer"], 
                                                                                          ValidateAudience = true, ValidAudience = jwt["Audience"], 
                                                                                          ValidateIssuerSigningKey = true, 
                                                                                          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!)) }); 
builder.Services.AddAuthorization(); 
var app = builder.Build(); 
using (var scope = app.Services.CreateScope()) 
    scope.ServiceProvider.GetRequiredService<BookingsDbContext>().Database.Migrate(); 
app.UseSwagger(); 
app.UseSwaggerUI(); 
app.UseAuthentication(); 
app.UseAuthorization(); 
app.MapControllers(); 
app.MapPrometheusScrapingEndpoint();
app.Run();
