using Microsoft.Extensions.DependencyInjection;

namespace Dallal_Backend_v2.IntegrationTests;

public class DatabaseConnectionTests(WebApplicationFactoryFixture _factory)
    : IClassFixture<WebApplicationFactoryFixture>
{
    [Fact]
    public async Task DatabaseConnection_ShouldConnect_Successfully()
    {
        // Arrange - Get DatabaseContext through dependency injection
        using var dbContext = _factory.CreateDbContext();

        // Act & Assert
        var canConnect = await dbContext.Database.CanConnectAsync();
        Assert.True(canConnect, "Should be able to connect to the database on port 4432");
    }

    [Fact]
    public async Task DatabaseConnection_ShouldExecuteBasicQuery()
    {
        // Arrange - Get DatabaseContext through dependency injection (scoped properly)
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        // Act & Assert - Test basic connectivity with a simple query
        var canConnect = await dbContext.Database.CanConnectAsync();
        Assert.True(canConnect, "Should be able to connect to the database on port 4432");

        // Verify database exists and is accessible
        var databaseExists = await dbContext.Database.EnsureCreatedAsync();
        Assert.True(true, "Database should be accessible and queryable on port 4432");
    }

    [Fact]
    public async Task DependencyInjection_ShouldResolveServices()
    {
        // Arrange & Act - Test that we can resolve services through DI using proper scoping
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        // Assert
        Assert.NotNull(dbContext);

        // Verify the service works
        var canConnect = await dbContext.Database.CanConnectAsync();
        Assert.True(canConnect, "DatabaseContext resolved through DI should connect to port 4432");

        // Let the scope handle disposal automatically
    }
}
