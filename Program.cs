using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using dotenv.net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using minimalAPI.Data;
using minimalAPI.EndPoints;
using minimalAPI.Extension;
using minimalAPI.Middleware;
using minimalAPI.Services;
using minimalAPI.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

dotenv.net.DotEnv.Load(options: new dotenv.net.DotEnvOptions(
  probeForEnv: true));

var builder = WebApplication.CreateBuilder(args);

 

//Registro de autenticacion JWT  
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["KEYCLOAK_AUTHORITY"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["KEYCLOAK_ISSUER"],
            RoleClaimType = ClaimTypes.Role,
            ValidateAudience = true,
            ValidAudiences = new[] { "document-services", "account" },
            ValidateIssuer = true
        };
    });

//Definicion de politica de autenticacion 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantAccess", policy => policy.RequireClaim("tenantid"));
    options.AddPolicy("CreateAccess", policy => policy.RequireRole("prueba:create"));
    options.AddPolicy("CanRead", policy => policy.RequireRole("prueba:read"));
    options.AddPolicy("DeleteAccess", policy => policy.RequireRole("prueba:delete"));
    options.AddPolicy("CreateClientAccess", policy => policy.RequireRole("prueba:cliente:create"));
    options.AddPolicy("ClienteCanRead", policy => policy.RequireRole("prueba:cliente:read"));
    options.AddPolicy("ClienteDeleteAccess", policy => policy.RequireRole("prueba:cliente:delete"));
    options.AddPolicy("ClienteDownLoadAccess", policy => policy.RequireRole("prueba:cliente:download"));
});

builder.Services.AddAuthorization();

//output cache
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(5))
        .Tag("documents");
    });
    options.AddPolicy("AuthenticatedPolicy", new AuthenticatedCache());
});

// Configuración de DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetSection("DB_CONNECTION").Value);
});

// Registro de servicios  
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IContainerServices, ContainerServices>();
builder.Services.AddScoped<IBlobServices, BlobServices>();
builder.Services.AddTransient<IClaimsTransformation, KeycloakRoleTransform>();

// Registro del BlobServiceClient 
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["AZURE_STORAGE_CONNECTION"];
    return new BlobServiceClient(connectionString);
});



var app = builder.Build();

 
app.UseAuthentication();
app.UseAuthorization();
app.UseExtensionMiddleware();  
app.UseOutputCache();          


app.MapContainerEndPoints();
app.MapBlobEndpoints();


app.Run();