// Program.cs
using PalmOasis_Limpieza_Push.Models;           // LimpiezaContext
using PalmOasis_Limpieza_Push.Models.Interfaces;
using PalmOasis_Limpieza_Push.Services;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ───────────────────────────────────────────────────────────────────────────────
// Cargar configuración (appsettings.json)
// ───────────────────────────────────────────────────────────────────────────────
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
	config.SetBasePath(AppContext.BaseDirectory);
	config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
	// Si quieres, añade appsettings.{Environment}.json, user-secrets, etc.
});

// ───────────────────────────────────────────────────────────────────────────────
// Ejecutar como servicio de Windows (opcional, quita si no lo necesitas)
// ───────────────────────────────────────────────────────────────────────────────
builder.Host.UseWindowsService();

// ───────────────────────────────────────────────────────────────────────────────
// Configurar Kestrel: HTTP y HTTPS con PFX
// ───────────────────────────────────────────────────────────────────────────────
// Ajusta los puertos y la ruta del PFX/contraseña a tu entorno.
builder.WebHost.ConfigureKestrel(options =>
{
	// HTTP
	options.ListenAnyIP(7059);

});

// ───────────────────────────────────────────────────────────────────────────────
// Servicios
// ───────────────────────────────────────────────────────────────────────────────

// EF Core → SQL Server (cadena en appsettings.json: ConnectionStrings:Default)
builder.Services.AddDbContext<LimpiezaContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Controllers
builder.Services.AddControllers();

// CORS (política similar a la que usaste en RRHH)
builder.Services.AddCors(opt =>
{
	opt.AddPolicy("PoliticaPalmOasis", policy =>
		policy.AllowAnyOrigin()
			  .AllowAnyHeader()
			  .AllowAnyMethod());
});

// Configurar Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Push de Firebase
builder.Services.AddSingleton<IPushService, PushService>();

var app = builder.Build();


// CORS
app.UseCors("PoliticaPalmOasis");

// (Si más tarde añades autenticación/autorización, colócalas aquí)
app.UseAuthorization();

// Controllers
app.MapControllers();

// Run
app.Run();
