using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using Users.Application;
using Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: configuration["ServiceName"]!))
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
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UsersDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("UsersDb")));
builder.Services.AddScoped<IUserRepository, UserRepository>(); 
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>(); 
builder.Services.AddScoped<ITokenService, JwtTokenService>(); 
builder.Services.AddScoped<IUserService, UserService>();
var app = builder.Build(); 
using (var scope = app.Services.CreateScope()) 
    scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.Migrate();
app.UseSwagger(); 
app.UseSwaggerUI(); 
app.MapControllers(); 
app.MapPrometheusScrapingEndpoint();
app.Run();
