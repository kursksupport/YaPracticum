using System.Text; 
using Events.Application; 
using Events.Infrastructure; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args); 
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
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
builder.Services.AddDbContext<EventsDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("EventsDb"))); 
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");
builder.Services.AddRedis(redisConnectionString);
builder.Services.AddScoped<IEventRepository, EventRepository>(); 
builder.Services.AddScoped<IEventService, EventService>(); 
builder.Services.AddHostedService<KafkaTopicInitializer>();
builder.Services.AddHostedService<BookingConfirmedConsumer>();
var jwt = builder.Configuration.GetSection("Jwt"); 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x => x.TokenValidationParameters = new() 
                                                                                        { ValidateIssuer = true, 
                                                                                          ValidIssuer = jwt["Issuer"], 
                                                                                          ValidateAudience = true, 
                                                                                          ValidAudience = jwt["Audience"], 
                                                                                          ValidateIssuerSigningKey = true, 
                                                                                          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!)) }); 
builder.Services.AddAuthorization(); 
var app = builder.Build(); 
using (var scope = app.Services.CreateScope()) 
    scope.ServiceProvider.GetRequiredService<EventsDbContext>().Database.Migrate(); 
app.UseSwagger(); 
app.UseSwaggerUI(); 
app.UseAuthentication(); 
app.UseAuthorization(); 
app.MapControllers(); 
app.Run();
