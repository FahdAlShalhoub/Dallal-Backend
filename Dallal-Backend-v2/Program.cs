using System.Diagnostics;
using System.Text.Json.Serialization;
using Dallal_Backend_v2;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to convert enums to strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

if (Environment.GetEnvironmentVariable("EF_BUNDLE_EXECUTION") != "true")
{
    string? databaseConnectionString = builder.Configuration.GetConnectionString("Default");
    Trace.Assert(
        !string.IsNullOrEmpty(databaseConnectionString),
        "Database connection string not found"
    );
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(databaseConnectionString);
    dataSourceBuilder.EnableDynamicJson();
    dataSourceBuilder.UseNetTopologySuite();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<DatabaseContext>(opt =>
        opt.UseNpgsql(dataSource, optionsBuilder => optionsBuilder.UseNetTopologySuite())
            .UseSeeding(DatabaseContext.Seed())
    );

    string? jwtSecret = builder.Configuration.GetRequiredSection("JWT")["SecretKey"];
    string? issuer = builder.Configuration.GetRequiredSection("JWT")["Issuer"];
    Trace.Assert(!string.IsNullOrEmpty(jwtSecret), "JWTSecret not found");
    Trace.Assert(!string.IsNullOrEmpty(issuer), "JWT Issuer not found");
    var jwt = new JwtService(jwtSecret, issuer);
    builder.Services.AddSingleton(jwt);
    builder
        .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => { options.TokenValidationParameters = jwt.GetTokenValidationParameters(); });

    string? firebaseAuth = builder.Configuration.GetRequiredSection("Firebase")["ServiceAccount"];
    Trace.Assert(!string.IsNullOrEmpty(firebaseAuth), "Firebase Service Account not found");
    builder.Services.AddSingleton(new FirebaseTokenVerifier(firebaseAuth));

    string? containerName = builder.Configuration.GetRequiredSection("S3")["ContainerName"];
    Trace.Assert(!string.IsNullOrEmpty(containerName), "S3 container not found");
    string? region = builder.Configuration.GetRequiredSection("S3")["Region"];
    Trace.Assert(!string.IsNullOrEmpty(region), "S3 region not found");
    string? secretAccessKey = builder.Configuration.GetRequiredSection("S3")["SecretAccessKey"];
    Trace.Assert(!string.IsNullOrEmpty(secretAccessKey), "S3 secret access key not found");
    string? accessKeyId = builder.Configuration.GetRequiredSection("S3")["AccessKeyId"];
    Trace.Assert(!string.IsNullOrEmpty(accessKeyId), "S3 access key id not found");
    builder.Services.AddSingleton(new S3(containerName, region, secretAccessKey, accessKeyId));

    builder.Services.AddScoped<SubmissionService>();
}
else
{
    builder.Services.AddDbContext<DatabaseContext>(opt =>
        opt.UseNpgsql(optionsBuilder => optionsBuilder.UseNetTopologySuite()));
}

builder.Services.Configure<RequestLocalizationOptions>(i =>
{
    i.SupportedCultures = [new("en"), new("ar")];
    i.SupportedUICultures = [new("en"), new("ar")];
});

// Configure HTTP JSON options for consistent enum serialization
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthorization(i => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();
app.UseMiddleware<RequestLocalizationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.MapControllers().AllowAnonymous();
else
    app.MapControllers();

app.Run();