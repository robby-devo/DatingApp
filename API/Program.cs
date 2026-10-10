using System.Text;
using API.Data;
using API.Interfaces;
using API.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services.AddCors();
builder.Services.AddScoped<ITokenService, TokenService>();

// 1. TEACH THE APP HOW TO VERIFY JWT TOKENS
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = builder.Configuration["TokenKey"]
            ?? throw new Exception("TokenKey not found in appsettings");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,   // We don't have an external issuer URL yet
            ValidateAudience = false  // We don't have an audience URL yet
        };
    });

// Learn more about configuring OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors();

// 2. STATION THE BOUNCER IN THE PIPELINE (Order matters!)
app.UseAuthentication(); // Who are you? (reads the token)
app.UseAuthorization();  // Are you allowed in? (checks [Authorize])

app.MapControllers();

app.Run();