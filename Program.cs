using PalmOasis_Limpieza_Push.Models;           // LimpiezaContext
using PalmOasis_Limpieza_Push.Models.Interfaces;
using PalmOasis_Limpieza_Push.Services;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;

// OJO: aquí usamos WebApplicationOptions
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory, // carpeta del .dll/.exe
    Args = args
});

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

builder.Services.AddSingleton<IPushService, PushService>();

var app = builder.Build();

app.UseCors("PoliticaPalmOasis");

app.MapControllers();

app.Run();
