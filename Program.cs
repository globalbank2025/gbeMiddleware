using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;
using Microsoft.AspNetCore.Identity;
using GBEMiddlewareApi.Security;
using Microsoft.AspNetCore.Authorization;
using GBEMiddlewareApi.Security; // for PermissionAuthorizationHandler, BasicAuthHandler

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------
//  Register Authorization Services & Policies
// ----------------------------------------------------------------
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Can_create_vat", policy =>
        policy.Requirements.Add(new PermissionRequirement("Can_create_vat")));

    options.AddPolicy("Can_vat_approval", policy =>
        policy.Requirements.Add(new PermissionRequirement("Can_vat_approval")));

    options.AddPolicy("CanManageUsers", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanManageUsers")));

    options.AddPolicy("CanManageRoles", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanManageRoles")));

    options.AddPolicy("CanManageSystemSettings", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanManageSystemSettings")));

    options.AddPolicy("CanViewLogs", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanViewLogs")));

    // repeated?
    // options.AddPolicy("CanManageSystemSettings", policy =>
    //    policy.Requirements.Add(new PermissionRequirement("CanManageSystemSettings")));

    options.AddPolicy("CanCreateTransactions", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanCreateTransactions")));

    options.AddPolicy("CanApproveTransactions", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanApproveTransactions")));

    options.AddPolicy("CanRejectTransactions", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanRejectTransactions")));
});

// ----------------------------------------------------------------
//  Configure CORS
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
//  Configure Database & Identity Services
// ----------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register MiddlewareDbContext (for PartnerController, etc.)
builder.Services.AddDbContext<MiddlewareDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ----------------------------------------------------------------
//  Configure JWT Authentication & Token Validation
// ----------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("JWT");
var jwtSecret = jwtSettings["Secret"];
var key = Encoding.UTF8.GetBytes(jwtSecret);

// We use the "AddAuthentication()" call once, then chain .AddJwtBearer() and .AddScheme()
builder.Services.AddAuthentication(options =>
{
    // Keep existing default for portal (JWT Bearer)
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    // If you'd rather have a "multi-scheme" approach, you could define a policy scheme.
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
})
// Add BasicAuth scheme
.AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>(
    "BasicAuth",
    options => { /* you could configure scheme here if needed */ }
);

// ----------------------------------------------------------------
//  Register Controllers & Swagger
// ----------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register an HttpClient for any SOAP calls
builder.Services.AddHttpClient();

// ----------------------------------------------------------------
//  Build & Configure Middleware Pipeline
// ----------------------------------------------------------------
var app = builder.Build();

app.UseStaticFiles();
app.UseCors("AllowAngularAndSwagger");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthentication();  // <--- must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
