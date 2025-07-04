using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Dallal_Backend_v2;

namespace Dallal_Backend_v2.IntegrationTests;

public class WebApplicationFactoryFixture : IDisposable
{
    private const string TestConnectionString = "Host=localhost;Port=4432;Username=root;Password=password;Database=dallal;";
    private readonly ServiceProvider _serviceProvider;
    private readonly WebApplicationFactory<Program>? _webApplicationFactory;
    
    public WebApplicationFactoryFixture()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        // Seed the database with mock data
        SeedDatabase();
        
        // Optionally create web application factory for HTTP testing
        try
        {
            _webApplicationFactory = new TestWebApplicationFactory();
        }
        catch
        {
            // If web app factory fails due to missing dependencies, continue with just DI testing
            _webApplicationFactory = null;
        }
    }
    
    public DatabaseContext CreateDbContext()
    {
        return _serviceProvider.GetRequiredService<DatabaseContext>();
    }

    public T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public T? GetOptionalService<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }

    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register the data source as a singleton
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(TestConnectionString);
        dataSourceBuilder.EnableDynamicJson();
        dataSourceBuilder.UseNetTopologySuite();
        var dataSource = dataSourceBuilder.Build();
        
        services.AddSingleton(dataSource);

        // Configure test database context using dependency injection
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseNpgsql(dataSource, optionsBuilder => 
                optionsBuilder.UseNetTopologySuite());
        });
    }

    private void SeedDatabase()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        
        // Ensure database is created
        dbContext.Database.EnsureCreated();
        
        // Check if data already exists to avoid duplicate seeding
        if (!dbContext.DetailsDefinitions.Any())
        {
            var seeder = DatabaseSeeder.Seed();
            seeder(dbContext, false);
        }
    }

    public HttpClient CreateClient()
    {
        if (_webApplicationFactory != null)
        {
            return _webApplicationFactory.CreateClient();
        }
        
        // Fallback for DI-only testing
        throw new InvalidOperationException("Web application factory not available. HTTP testing requires full application setup.");
    }

    public void Dispose()
    {
        _webApplicationFactory?.Dispose();
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString = "Host=localhost;Port=4432;Username=root;Password=password;Database=dallal;";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true);
        });
        
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Register the data source as a singleton
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(TestConnectionString);
            dataSourceBuilder.EnableDynamicJson();
            dataSourceBuilder.UseNetTopologySuite();
            var dataSource = dataSourceBuilder.Build();
            
            services.AddSingleton(dataSource);

            // Configure test database context using dependency injection
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(dataSource, optionsBuilder => 
                    optionsBuilder.UseNetTopologySuite());
            });
        });
    }
}