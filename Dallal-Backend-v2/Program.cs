using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
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
using Serilog.Sinks.Loki;

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
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = jwt.GetTokenValidationParameters();
        });

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

    string? lokiUrl = builder.Configuration.GetRequiredSection("Loki")["Uri"];
    Trace.Assert(!string.IsNullOrEmpty(lokiUrl), "Loki url not found");
    string? lokiUsername = builder.Configuration.GetRequiredSection("Loki")["Username"];
    Trace.Assert(!string.IsNullOrEmpty(lokiUsername), "Loki username not found");
    string? lokiPassword = builder.Configuration.GetRequiredSection("Loki")["Password"];
    Trace.Assert(!string.IsNullOrEmpty(lokiPassword), "Loki Password not found");

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Service_Name", "Dallal_Backend")
        .WriteTo.Console()
        .WriteTo.LokiHttp(new BasicAuthCredentials(lokiUrl, lokiUsername, lokiPassword))
        .CreateLogger();

    builder.Host.UseSerilog();

    string? tempoUrl = builder.Configuration.GetRequiredSection("Tempo")["Uri"];
    Trace.Assert(!string.IsNullOrEmpty(tempoUrl), "Tempo url not found");
    string? tempoUsername = builder.Configuration.GetRequiredSection("Tempo")["Username"];
    Trace.Assert(!string.IsNullOrEmpty(tempoUsername), "Tempo username not found");
    string? tempoPassword = builder.Configuration.GetRequiredSection("Tempo")["Password"];
    Trace.Assert(!string.IsNullOrEmpty(tempoPassword), "Tempo Password not found");

    builder
        .Services.AddOpenTelemetry()
        .WithTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .AddSource("Dallal.Backend")
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("http.request.body.size", httpRequest.ContentLength);
                        activity.SetTag(
                            "http.request.header.user-agent",
                            httpRequest.Headers.UserAgent.ToString()
                        );
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("http.response.body.size", httpResponse.ContentLength);
                    };
                    options.Filter = httpContext =>
                    {
                        // Filter out health check and static file requests
                        var path = httpContext.Request.Path.Value;
                        return !path?.Contains("health") == true
                            && !path?.Contains("swagger") == true
                            && !path?.Contains("scalar") == true;
                    };
                })
                // Add HTTP client instrumentation
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                    {
                        activity.SetTag("http.request.method", httpRequestMessage.Method.Method);
                        activity.SetTag(
                            "http.request.uri",
                            httpRequestMessage.RequestUri?.ToString()
                        );
                    };
                    options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                    {
                        activity.SetTag(
                            "http.response.status_code",
                            (int)httpResponseMessage.StatusCode
                        );
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                    options.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.statement_type", command.CommandType.ToString());
                    };
                })
                .SetResourceBuilder(
                    ResourceBuilder
                        .CreateDefault()
                        .AddService(
                            serviceName: "dallal-backend",
                            serviceVersion: "1.0.0",
                            serviceInstanceId: Environment.MachineName
                        )
                        .AddAttributes(
                            new Dictionary<string, object>
                            {
                                ["deployment.environment"] = builder.Environment.EnvironmentName,
                                ["service.namespace"] = "dallal",
                                ["host.name"] = Environment.MachineName,
                                ["os.type"] = Environment.OSVersion.Platform.ToString(),
                                ["runtime.name"] = ".NET",
                                ["runtime.version"] = Environment.Version.ToString(),
                            }
                        )
                )
                .SetSampler(new TraceIdRatioBasedSampler(1.0)) // Sample 100% in development, adjust for production
                // .AddConsoleExporter()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(tempoUrl);
                    var credentials = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{tempoUsername}:{tempoPassword}")
                    );
                    options.Headers = $"Authorization=Basic {credentials}";
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        });
}
else
{
    builder.Services.AddDbContext<DatabaseContext>(opt =>
        opt.UseNpgsql(optionsBuilder => optionsBuilder.UseNetTopologySuite())
    );
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
