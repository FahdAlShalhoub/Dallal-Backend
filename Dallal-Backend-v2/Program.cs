using System.Diagnostics;
using Dallal_Backend_v2;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Google.Apis.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();


if (Environment.GetEnvironmentVariable("EF_BUNDLE_EXECUTION") != "true")
{
    string? databaseConnectionString = builder.Configuration.GetConnectionString("Default");
    Trace.Assert(!string.IsNullOrEmpty(databaseConnectionString), "Database connection string not found");
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(databaseConnectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<DatabaseContext>(opt =>
        opt.UseNpgsql(dataSource).UseSeeding(DatabaseContext.Seed()));

    string? jwtSecret = builder.Configuration.GetRequiredSection("JWT")["SecretKey"];
    Trace.Assert(!string.IsNullOrEmpty(jwtSecret), "JWTSecret not found");
    var jwt = new JwtService(jwtSecret);
    builder.Services.AddSingleton(jwt);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => { options.TokenValidationParameters = jwt.GetTokenValidationParameters(); });

    string? firebaseAuth = builder.Configuration.GetRequiredSection("Firebase")["ServiceAccount"];
    Trace.Assert(!string.IsNullOrEmpty(firebaseAuth), "Firebase Service Account not found");
    builder.Services.AddSingleton(new FirebaseTokenVerifier(firebaseAuth));
}
else
{
    builder.Services.AddDbContext<DatabaseContext>(opt =>
        opt.UseNpgsql());
}


builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();