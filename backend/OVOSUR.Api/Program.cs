using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OVOSUR.Api.Infrastructure.Persistence;
using OVOSUR.Api.Modules.Security;
using OVOSUR.Api.Modules.Security.Entities;
using OVOSUR.Api.Modules.Security.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<OvosurDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OvosurIntranet")));
builder.Services.AddScoped<PasswordHasher<Usuario>>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("OvosurWeb", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("OvosurWeb");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "OVOSUR.Api",
    status = "OK",
    utc = DateTimeOffset.UtcNow
}));

app.MapGet("/api/platform/modules", () => Results.Ok(new[]
{
    new { code = "PROVEEDORES", name = "Proveedores", status = "planned" },
    new { code = "VENTAS", name = "Ventas", status = "planned" },
    new { code = "TESORERIA", name = "Tesoreria", status = "planned" },
    new { code = "DISTRIBUCION", name = "Distribucion", status = "planned" }
}));
app.MapAuthEndpoints();

app.Run();
