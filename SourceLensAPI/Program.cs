using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Connect Entity Framework Core to SQL Server
builder.Services.AddDbContext<SourceLensDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SourceLensDb")));

// Add services to the container
builder.Services.AddControllers();

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();