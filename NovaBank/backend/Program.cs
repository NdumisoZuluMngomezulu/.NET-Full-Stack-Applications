using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NovaBank.Data;
using NovaBank.DTOs;
using NovaBank.Models;
using NovaBank.Services;
using System.Security.Cryptography;
using System.Reflection.Metadata;
using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BankDatabase")));
builder.Services.AddIdentityCore<BankUser>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<BankDbContext>().AddSignInManager();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Set Jwt:Key in user secrets or environment variables.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true, IssuerSigningKey = signingKey,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<BankingService>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<BankUser> users, BankDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Name, email and password are required."});
    }
    var user = new BankUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), FullName = request.FullName.Trim() };
    var result = await users.CreateAsync(user, request.Password);
    if (!result.Succeded) return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    db.Accounts.Add(new BankAccount { UserStringHandle = user.Id, AccountNumber = GeneratedAccountNumber(), Balance = 1000m, Currency = "ZAR"});
    await db.SaveChangesAsync();
    return Results.Created("/api/accounts/me", new { message = "Demo acccount with R1,000.00 demo funds"});
}).AllowAnonymous();

app.MapPost("/api/auth/login", async (LoginRequest request, UserManager<BankUser> users, IConfiguration config) =>
{
    var user = await users.FindByEmailAsync(request.Email.Trim());
    if (user is null || !await users.CheckPasswordAsync(user, request.Password))
    {
        return Results.Unauthorized();
    }
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Email, user.Email ?? "")};
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var token = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"], claims,
        expires: DateTime.UtcNow.AddMinutes(30), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );
    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), fullName = user.FullName, expiresInMinutes = 30});
}).AllowAnonymous();

app.MapGet("/api/accounts/me", async (ClaimsPrincipal principal, BankingService banking) => 
    Results.Ok(await banking.GetMyAccounts(principal.FindFirstValue(ClaimTypes.NameIdentifier)!))
).RequireAuthorization();

app.MapGet("/api/transactions", async (ClaimsPrincipal principal, BankingService banking) =>
    Results.Ok(await banking.GetMyTransactions(principal.FindFirstValue(ClaimTypes.NameIdentifier)!))
).RequireAuthorization();

app.MapPost("/api/trasnfers", async (TransferRequest request, ClaimsPrincipal principal, BankingService banking) =>
{
    var result = await banking.Transfer(principal.FindFirstValue(ClaimTypes.NameIdentifier)!, request);
    return result.Success ? Results.Ok(new { messages = result.Message, reference = result.Reference }) : Results.BadRequest(new { message = result.Message });
}).RequireAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", app = "NovaBank demo"})
);
app.Run();

static string GenerateAccountNumber() => Random.Shared.NextInt64(1000000000, 9999999999).ToString();





// app.MapGet("/api/accounts/me", async (ClaimsPrincipal principal, BankingService banking) =>
//     Results.Ok(await banking.GetMyAccounts(principal.FindFirstValue(ClaimTypes.NameIdentifier)!))).RequireAuthorization();
// app.MapGet("/api/transactions", async (ClaimsPrincipal principal, BankingService banking) =>
//     Results.Ok(await banking.GetMyTransactions(principal.FindFirstValue(ClaimTypes.NameIdentifier)!))).RequireAuthorization();
// app.MapPost("/api/transfers", async (TransferRequest request, ClaimsPrincipal principal, BankingService banking) => {
//     var result = await banking.Transfer(principal.FindFirstValue(ClaimTypes.NameIdentifier)!, request);
//     return result.Success ? Results.Ok(new { message = result.Message, reference = result.Reference }) : Results.BadRequest(new { message = result.Message });
// }).RequireAuthorization();
// app.MapGet("/api/health", () => Results.Ok(new { status = "ok", app = "NovaBank demo" }));
// app.Run();

// static string GenerateAccountNumber() => Random.Shared.NextInt64(1000000000, 9999999999).ToString();
