using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;
using Microsoft.AspNetCore.Identity;
using GBEMiddlewareApi.Security;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Register your custom authorization handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Register authorization policies
builder.Services.AddAuthorization(options =>
{
    // Policy for creating VAT collection transactions (Maker permission)
    options.AddPolicy("test", policy =>
        policy.Requirements.Add(new PermissionRequirement("test")));
    // Policy for creating VAT collection transactions (Maker permission)
    options.AddPolicy("VatCollection_Create", policy =>
        policy.Requirements.Add(new PermissionRequirement("VatCollection_Create")));

    // Policy for approving VAT collection transactions (Checker permission)
    options.AddPolicy("VatCollection_Approve", policy =>
        policy.Requirements.Add(new PermissionRequirement("VatCollection_Approve")));
});

// ----------------------------------------------------------------
// Add CORS service and define a policy to allow Angular and Swagger.
// For development, we're using AllowAnyOrigin. In production, consider
// specifying exact origins for security.
// ----------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularAndSwagger", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ----------------------------------------------------------------
// Configure connection strings and JWT settings
// ----------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSettings = builder.Configuration.GetSection("JWT");
var jwtSecret = jwtSettings["Secret"];
var key = Encoding.UTF8.GetBytes(jwtSecret);

// ----------------------------------------------------------------
// Register ApplicationDbContext for Identity (using ApplicationUser)
// ----------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ----------------------------------------------------------------
// Register Identity with ApplicationUser and IdentityRole
// ----------------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ----------------------------------------------------------------
// Register MiddlewareDbContext for common data tables (if needed)
// ----------------------------------------------------------------
builder.Services.AddDbContext<MiddlewareDbContext>(options =>
    options.UseNpgsql(connectionString));

// ----------------------------------------------------------------
// Configure JWT Authentication
// ----------------------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["ValidIssuer"],
        ValidAudience = jwtSettings["ValidAudience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

// ----------------------------------------------------------------
// Register HttpClient so it can be injected into controllers
// ----------------------------------------------------------------
builder.Services.AddHttpClient();

// ----------------------------------------------------------------
// Register controllers and Swagger
// ----------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ----------------------------------------------------------------
// Use static files (if any)
// ----------------------------------------------------------------
app.UseStaticFiles();

// ----------------------------------------------------------------
// Use CORS with the defined policy. This MUST be placed before
// UseAuthentication/UseAuthorization.
// ----------------------------------------------------------------
app.UseCors("AllowAngularAndSwagger");

// ----------------------------------------------------------------
// Enable Swagger (consider securing Swagger in production)
// ----------------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Swagger UI will appear at the root URL.
});

// ----------------------------------------------------------------
// Enforce HTTPS redirection
// ----------------------------------------------------------------
app.UseHttpsRedirection();

// ----------------------------------------------------------------
// Use Authentication and Authorization
// ----------------------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------------------
// Map controller endpoints
// ----------------------------------------------------------------
app.MapControllers();

app.Run();
