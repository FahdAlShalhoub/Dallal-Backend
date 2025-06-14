using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Dallal_Backend_v2;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;

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

//
// // Configure Serilog
// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(builder.Configuration)
//     .Enrich.FromLogContext()
//     .Enrich.WithEnvironmentName()
//     .Enrich.WithMachineName()
//     .Enrich.WithProcessId()
//     .Enrich.WithThreadId()
//     .WriteTo.Console()
//     .WriteTo.LokiHttp(
//         uri: "https://logs-prod-006.grafana.net", // Replace with your Loki URL
//         credentials: new LokiCredentials()
//         {
//             Login = "your-loki-username", // Replace with your Loki username
//             Password = "your-loki-password" // Replace with your Loki password
//         },
//         labels: new List<LokiLabel>
//         {
//             new() { Key = "app", Value = "dallal-backend" },
//             new() { Key = "environment", Value = builder.Environment.EnvironmentName },
//             new() { Key = "version", Value = "1.0.0" }
//         })
//     .CreateLogger();
//
// // Add Serilog to the container
// builder.Host.UseSerilog();
//
// builder.Services.AddAuthorization(i => { });
//
// builder.Services.AddOpenTelemetry()
//     .WithTracing(tracerProviderBuilder =>
//     {
//         tracerProviderBuilder
//             .AddSource("MyApp") // Your application name
//             .SetResourceBuilder(ResourceBuilder.CreateDefault()
//                 .AddService("my-dotnet-app", "1.0.0")
//                 .AddTelemetrySdk())
//             .AddAspNetCoreInstrumentation(options =>
//             {
//                 options.RecordException = true;
//                 options.Filter = httpContext =>
//                 {
//                     // Filter out health check requests
//                     return !httpContext.Request.Path.Value?.Contains("health") == true;
//                 };
//             })
//             .AddHttpClientInstrumentation(options =>
//             {
//                 options.RecordException = true;
//             })
//             .AddConsoleExporter()
//             .AddOtlpExporter(options =>
//             {
//                 options.Endpoint = new Uri("https://otlp-gateway-prod-me-central-1.grafana.net/otlp/v1/traces");
//                 options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
//
//                 // Add Basic Authentication
//                 var username = "1213268";
//                 var password =
//                     "glc_eyJvIjoiMTQzMzAzOCIsIm4iOiJwZGMtc3R1ZGlvczk4c3RhZ2luZy1kZWZhdWx0LXRlbXBvLXJlZ2lzdHJhdGlvbnMtc3ZjIiwiayI6IjhONWRqbDlwZmdoVjVuM0I4QTJnNzlZMSIsIm0iOnsiciI6InByb2QtbWUtY2VudHJhbC0xIn19";
//
//                 if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
//                 {
//                     var credentials = Convert.ToBase64String(
//                         Encoding.UTF8.GetBytes($"{username}:{password}"));
//
//                     options.Headers = $"Authorization=Basic {credentials}";
//                 }
//
//             });
//     });

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