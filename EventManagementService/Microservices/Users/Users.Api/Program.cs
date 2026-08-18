using Microsoft.EntityFrameworkCore;
using Users.Application;
using Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
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
app.Run();
