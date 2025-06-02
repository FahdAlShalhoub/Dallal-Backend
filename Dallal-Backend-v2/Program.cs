using System.Diagnostics;
using Dallal_Backend_v2;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

string? jwtSecret = builder.Configuration.GetRequiredSection("JWT")["SecretKey"];
Trace.Assert(!string.IsNullOrEmpty(jwtSecret), "JWTSecret not found");

string? firebaseAuth = builder.Configuration.GetRequiredSection("Firebase")["ServiceAccount"];
Trace.Assert(!string.IsNullOrEmpty(firebaseAuth), "Firebase Service Account not found");

string? databaseConnectionString = builder.Configuration.GetConnectionString("Default");
Trace.Assert(!string.IsNullOrEmpty(databaseConnectionString), "Database connection string not found");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DatabaseContext>(opt =>
    opt.UseNpgsql(databaseConnectionString));

var jwt = new JwtService(jwtSecret);
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton(new FirebaseTokenVerifier(firebaseAuth));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { options.TokenValidationParameters = jwt.GetTokenValidationParameters(); });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();