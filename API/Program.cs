using API.Data;
using Microsoft.EntityFrameworkCore;

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

// Learn more about configuring OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors();

app.MapControllers();

app.Run();