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
using Microsoft.Extensions.Hosting.WindowsServices;


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseWindowsService();


builder.WebHost.ConfigureKestrel((context, options) =>
{
	options.Configure(context.Configuration.GetSection("Kestrel"));
});

builder.Services.AddDbContext<LimpiezaContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddControllers();

builder.Services.AddCors(opt =>
{
	opt.AddPolicy("PoliticaPalmOasis", policy =>
		policy.AllowAnyOrigin()
			  .AllowAnyHeader()
			  .AllowAnyMethod());
});

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

app.UseAuthorization();

// Controllers
app.MapControllers();

// Run
app.Run();
