// Program.cs – ASP.NET Core entry point for the Clinic Management System.
//
// cr-dotnet-0045 remediation:
//   Registers Amazon ElastiCache for Redis as the distributed session store via
//   AddStackExchangeRedisCache().  This replaces the in-process (InProc) session
//   provider that was flagged by rule cr-dotnet-0045.
//
//   The Redis connection string is read from the REDIS_CONNECTION_STRING environment
//   variable (set in ECS task definition / Kubernetes secret) so that no credentials
//   are hardcoded in source code (12-factor app principle III).
//
//   Session middleware (AddSession / UseSession) is configured to use the Redis-backed
//   IDistributedCache, enabling stateless horizontal scaling across multiple ECS tasks
//   or Kubernetes pods without server affinity.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
// appsettings.json values can be overridden by environment variables at runtime.
IConfiguration configuration = builder.Configuration;

// ── Razor Pages ──────────────────────────────────────────────────────────────
builder.Services.AddRazorPages();

// ── Amazon ElastiCache for Redis – Distributed Session Store ─────────────────
// cr-dotnet-0045: Replace in-process (InProc) session with Redis distributed cache.
// Connection string is supplied via the REDIS_CONNECTION_STRING environment variable
// (e.g. "my-cluster.abc123.ng.0001.use1.cache.amazonaws.com:6379").
// Falls back to the appsettings.json Redis:ConnectionString value for local development.
string redisConnectionString =
    Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
    ?? configuration["Redis:ConnectionString"]
    ?? "localhost:6379";

string redisInstanceName =
    Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME")
    ?? configuration["Redis:InstanceName"]
    ?? "ClinicMgmt_";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName  = redisInstanceName;
});

// ── Session middleware ────────────────────────────────────────────────────────
// Uses the Redis-backed IDistributedCache registered above.
int idleTimeoutMinutes = int.TryParse(
    Environment.GetEnvironmentVariable("REDIS_SESSION_IDLE_MINUTES")
    ?? configuration["Session:IdleTimeoutMinutes"],
    out int parsed) ? parsed : 20;

builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(idleTimeoutMinutes);
    options.Cookie.Name        = configuration["Session:CookieName"] ?? ".ClinicMgmt.Session";
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite    = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// UseSession MUST be called before MapRazorPages so that session is available
// in all Razor Page handlers.
app.UseSession();

app.UseAuthorization();
app.MapRazorPages();

app.Run();
