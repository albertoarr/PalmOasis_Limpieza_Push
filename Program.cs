using Microsoft.EntityFrameworkCore;
using PalmOasis_Limpieza_Push.Services;
using PalmOasis_Limpieza_Push.Models;
using PalmOasis_Limpieza_Push.Models.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Servicio de conexión a BD
builder.Services.AddDbContext<LimpiezaContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("Recibos")));

// Servicios del builder
builder.Services.AddControllers();

// Servicio de Push con Firebase
builder.Services.AddSingleton<IPushService, PushService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
