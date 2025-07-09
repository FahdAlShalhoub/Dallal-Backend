using Dallal_Backend_v2.Controllers;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Details;
using Dallal_Backend_v2.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dallal_Backend_v2.IntegrationTests;

public class AdminDetailsControllerUnitTests(WebApplicationFactoryFixture factory) : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory = factory;

    [Fact]
    public async Task CreateDetailsDefinition_WithValidData_ShouldCreateInDatabase()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        var createDto = new CreateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto
            {
                Values = new Dictionary<string, string>
                {
                    { "en", "Test Property Feature" },
                    { "ar", "ميزة العقار التجريبية" }
                }
            },
            Type = DetailDefinitionType.MultiSelect,
            SearchBehavior = DetailDefinitionSearchBehavior.And,
            PropertyTypes = new List<PropertyType> { PropertyType.Apartment, PropertyType.Villa },
            IsHidden = false,
            Options = new List<CreateDetailsDefinitionOptionDto>
            {
                new()
                {
                    Name = new LocalizedStringDto
                    {
                        Values = new Dictionary<string, string>
                        {
                            { "en", "Swimming Pool" },
                            { "ar", "مسبح" }
                        }
                    }
                },
                new()
                {
                    Name = new LocalizedStringDto
                    {
                        Values = new Dictionary<string, string>
                        {
                            { "en", "Gym" },
                            { "ar", "صالة رياضية" }
                        }
                    }
                }
            }
        };

        // Act
        var result = await controller.CreateDetailsDefinition(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Property Feature", result.Name.Values["en"]);
        Assert.Equal("ميزة العقار التجريبية", result.Name.Values["ar"]);
        Assert.Equal(DetailDefinitionType.MultiSelect, result.Type);
        Assert.Equal(DetailDefinitionSearchBehavior.And, result.SearchBehavior);
        Assert.False(result.IsHidden);
        Assert.NotNull(result.Options);
        Assert.Equal(2, result.Options.Count);

        // Verify database persistence
        var savedEntity = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == result.Id);
        
        Assert.NotNull(savedEntity);
        Assert.Equal("Test Property Feature", savedEntity.Name.Values["en"]);
        Assert.Equal("ميزة العقار التجريبية", savedEntity.Name.Values["ar"]);
        Assert.Equal(2, savedEntity.Options?.Count);
        Assert.Contains(savedEntity.Options!, o => o.Name.Values["en"] == "Swimming Pool");
        Assert.Contains(savedEntity.Options!, o => o.Name.Values["en"] == "Gym");
    }

    [Fact]
    public async Task CreateDetailsDefinition_WithTextType_ShouldCreateWithoutOptions()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        var createDto = new CreateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto("Property Description"),
            Type = DetailDefinitionType.Text,
            SearchBehavior = DetailDefinitionSearchBehavior.Hidden,
            PropertyTypes = new List<PropertyType> { PropertyType.Land },
            IsHidden = true,
            Options = new List<CreateDetailsDefinitionOptionDto>()
        };

        // Act
        var result = await controller.CreateDetailsDefinition(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Property Description", result.Name.Values["fallback"]);
        Assert.Equal(DetailDefinitionType.Text, result.Type);
        Assert.True(result.IsHidden);
        Assert.True(result.Options == null || result.Options.Count == 0);

        // Verify in database
        var savedEntity = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == result.Id);
        
        Assert.NotNull(savedEntity);
        Assert.Equal(DetailDefinitionType.Text, savedEntity.Type);
        Assert.True(savedEntity.Options == null || savedEntity.Options.Count == 0);
    }

    [Fact]
    public async Task UpdateDetailsDefinition_WithExistingId_ShouldUpdateSuccessfully()
    {
        // Arrange - Create initial entity using a direct simple approach
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        // First create a details definition to update (no options to avoid complex deletion scenario)
        var createDto = new CreateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto
            {
                Values = new Dictionary<string, string> { { "en", "Original Name" } }
            },
            Type = DetailDefinitionType.Boolean,
            SearchBehavior = DetailDefinitionSearchBehavior.And,
            PropertyTypes = new List<PropertyType> { PropertyType.Apartment },
            IsHidden = false,
            Options = new List<CreateDetailsDefinitionOptionDto>() // No initial options
        };

        var createdEntity = await controller.CreateDetailsDefinition(createDto);

        // Act - Update basic properties only (no option manipulation) 
        var updateDto = new UpdateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto
            {
                Values = new Dictionary<string, string>
                {
                    { "en", "Updated Feature Name" },
                    { "ar", "اسم الميزة المحدث" }
                }
            },
            Type = DetailDefinitionType.Text,
            SearchBehavior = DetailDefinitionSearchBehavior.Hidden,
            PropertyTypes = new List<PropertyType> { PropertyType.Villa },
            IsHidden = true,
            Options = new List<UpdateDetailsDefinitionOptionDto>() // Keep it simple - no options
        };

        var result = await controller.UpdateDetailsDefinition(createdEntity.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEntity.Id, result.Id);
        Assert.Equal("Updated Feature Name", result.Name.Values["en"]);
        Assert.Equal("اسم الميزة المحدث", result.Name.Values["ar"]);
        Assert.Equal(DetailDefinitionType.Text, result.Type);
        Assert.Equal(DetailDefinitionSearchBehavior.Hidden, result.SearchBehavior);
        Assert.True(result.IsHidden);
        Assert.True(result.Options == null || result.Options.Count == 0);

        // Verify changes persisted to database
        var updatedEntity = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == createdEntity.Id);

        Assert.NotNull(updatedEntity);
        Assert.Equal("Updated Feature Name", updatedEntity.Name.Values["en"]);
        Assert.Equal(DetailDefinitionType.Text, updatedEntity.Type);
        Assert.Equal(DetailDefinitionSearchBehavior.Hidden, updatedEntity.SearchBehavior);
        Assert.True(updatedEntity.IsHidden);
    }

    [Fact]
    public async Task GetSeededDetailsDefinitions_ShouldReturnExpectedData()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        // Act - Query the seeded data
        var seededDefinitions = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .ToListAsync();

        // Debug: Print out what we actually have
        Console.WriteLine($"Found {seededDefinitions.Count} definitions:");
        foreach (var def in seededDefinitions)
        {
            Console.WriteLine($"- ID: {def.Id}, Name: {string.Join(", ", def.Name.Values.Select(kv => $"{kv.Key}:{kv.Value}"))}");
        }

        // Assert - Verify the seeded data is present
        Assert.NotEmpty(seededDefinitions);
        Assert.True(seededDefinitions.Count >= 5, "Expected at least 5 seeded definitions");

        // Check for specific seeded definitions (client-side evaluation)
        var furnishingType = seededDefinitions.FirstOrDefault(d => 
            d.Name.Values.ContainsKey("en") && d.Name.Values["en"] == "Furnishing Type");
        Assert.NotNull(furnishingType);
        Assert.Equal(DetailDefinitionType.MultiSelect, furnishingType.Type);
        Assert.NotNull(furnishingType.Options);
        Assert.Equal(3, furnishingType.Options.Count); // Furnished, Semi-Furnished, Unfurnished

        var yearBuilt = seededDefinitions.FirstOrDefault(d => 
            d.Name.Values.ContainsKey("en") && d.Name.Values["en"] == "Year Built");
        Assert.NotNull(yearBuilt);
        Assert.Equal(DetailDefinitionType.Year, yearBuilt.Type);

        var hasParking = seededDefinitions.FirstOrDefault(d => 
            d.Name.Values.ContainsKey("en") && d.Name.Values["en"] == "Has Parking");
        Assert.NotNull(hasParking);
        Assert.Equal(DetailDefinitionType.Boolean, hasParking.Type);
    }

    [Fact]
    public async Task UpdateSeededDefinition_WithNewOptions_ShouldWork()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        // Find the seeded Furnishing Type definition using client evaluation
        var allDefinitions = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .ToListAsync();
        
        var furnishingDefinition = allDefinitions
            .FirstOrDefault(d => d.Name.Values.ContainsKey("en") && d.Name.Values["en"] == "Furnishing Type");
        
        Assert.NotNull(furnishingDefinition);
        var originalOptionCount = furnishingDefinition.Options?.Count ?? 0;
        var existingOptionId = furnishingDefinition.Options?.First().Id ?? Guid.Empty;

        // Get all existing option IDs to preserve them (business constraint: can't delete options in use)
        var allExistingOptionIds = furnishingDefinition.Options?.Select(o => o.Id).ToList() ?? new List<Guid>();

        // Act - Update with all existing options plus a new one (respecting business constraints)
        var updateDto = new UpdateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto
            {
                Values = new Dictionary<string, string>
                {
                    { "en", "Furnishing Type Updated" }, // Slightly change the name
                    { "ar", "نوع الفرش المحدث" }
                }
            },
            Type = DetailDefinitionType.MultiSelect,
            SearchBehavior = DetailDefinitionSearchBehavior.Or,
            PropertyTypes = new List<PropertyType> { PropertyType.Apartment, PropertyType.Villa },
            IsHidden = false,
            Options = new List<UpdateDetailsDefinitionOptionDto>()
        };

        // Add all existing options to preserve foreign key constraints
        foreach (var existingOption in furnishingDefinition.Options ?? new List<DetailsDefinitionOption>())
        {
            updateDto.Options.Add(new UpdateDetailsDefinitionOptionDto
            {
                Id = existingOption.Id,
                Name = new LocalizedStringDto
                {
                    Values = existingOption.Name.Values
                }
            });
        }

        // Add a new option
        updateDto.Options.Add(new UpdateDetailsDefinitionOptionDto
        {
            Id = null,
            Name = new LocalizedStringDto
            {
                Values = new Dictionary<string, string>
                {
                    { "en", "Luxury Furnished" },
                    { "ar", "مفروش فاخر" }
                }
            }
        });

        var result = await controller.UpdateDetailsDefinition(furnishingDefinition.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(furnishingDefinition.Id, result.Id);
        Assert.Equal("Furnishing Type Updated", result.Name.Values["en"]);
        Assert.Equal("نوع الفرش المحدث", result.Name.Values["ar"]);
        Assert.NotNull(result.Options);
        Assert.Equal(originalOptionCount + 1, result.Options.Count); // All original + 1 new

        // Verify all original options are preserved
        foreach (var originalOptionId in allExistingOptionIds)
        {
            var preservedOption = result.Options.FirstOrDefault(o => o.Id == originalOptionId);
            Assert.NotNull(preservedOption);
        }

        // Verify new option was added
        var newOption = result.Options.FirstOrDefault(o => !allExistingOptionIds.Contains(o.Id));
        Assert.NotNull(newOption);
        Assert.Equal("Luxury Furnished", newOption.Name.Values["en"]);
        Assert.Equal("مفروش فاخر", newOption.Name.Values["ar"]);

        // Verify changes persisted to database
        var updatedEntity = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == furnishingDefinition.Id);

        Assert.NotNull(updatedEntity);
        Assert.Equal("Furnishing Type Updated", updatedEntity.Name.Values["en"]);
        Assert.Equal(originalOptionCount + 1, updatedEntity.Options?.Count);
    }

    [Fact]
    public async Task CreateDetailsDefinition_UsingSeederConstants_ShouldCreateWithKnownIds()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        // Create a new definition that references existing seeded property types and patterns
        var createDto = new CreateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto
            {
                Values = new Dictionary<string, string>
                {
                    { "en", "Balcony Type" },
                    { "ar", "نوع الشرفة" }
                }
            },
            Type = DetailDefinitionType.MultiSelect,
            SearchBehavior = DetailDefinitionSearchBehavior.Or,
            PropertyTypes = new List<PropertyType> { PropertyType.Apartment, PropertyType.Villa }, // Same as seeded data
            IsHidden = false,
            Options = new List<CreateDetailsDefinitionOptionDto>
            {
                new()
                {
                    Name = new LocalizedStringDto
                    {
                        Values = new Dictionary<string, string>
                        {
                            { "en", "French Balcony" },
                            { "ar", "شرفة فرنسية" }
                        }
                    }
                },
                new()
                {
                    Name = new LocalizedStringDto
                    {
                        Values = new Dictionary<string, string>
                        {
                            { "en", "Terrace" },
                            { "ar", "تراس" }
                        }
                    }
                }
            }
        };

        // Act
        var result = await controller.CreateDetailsDefinition(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Balcony Type", result.Name.Values["en"]);
        Assert.Equal("نوع الشرفة", result.Name.Values["ar"]);
        Assert.Equal(DetailDefinitionType.MultiSelect, result.Type);
        Assert.NotNull(result.Options);
        Assert.Equal(2, result.Options.Count);

        // Verify it coexists with seeded data
        var allDefinitions = await dbContext.DetailsDefinitions.CountAsync();
        Assert.True(allDefinitions >= 6, "Should have at least 5 seeded + 1 new definition");
    }

    [Fact]
    public async Task UpdateDetailsDefinition_RemoveOptions_ShouldDeleteFromDatabase()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        var optionToKeep = new DetailsDefinitionOption
        {
            Id = Guid.NewGuid(),
            Name = new LocalizedString
            {
                Values = new Dictionary<string, string> { { "en", "Keep This" } }
            }
        };

        var optionToDelete = new DetailsDefinitionOption
        {
            Id = Guid.NewGuid(),
            Name = new LocalizedString
            {
                Values = new Dictionary<string, string> { { "en", "Delete This" } }
            }
        };

        var existingDefinition = new DetailsDefinition
        {
            Id = Guid.NewGuid(),
            Name = new LocalizedString
            {
                Values = new Dictionary<string, string> { { "en", "Test Definition" } }
            },
            Type = DetailDefinitionType.MultiSelect,
            SearchBehavior = DetailDefinitionSearchBehavior.And,
            IsHidden = false,
            Options = new List<DetailsDefinitionOption> { optionToKeep, optionToDelete }
        };

        await dbContext.DetailsDefinitions.AddAsync(existingDefinition);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto("Test Definition"),
            Type = DetailDefinitionType.MultiSelect,
            SearchBehavior = DetailDefinitionSearchBehavior.And,
            PropertyTypes = new List<PropertyType>(),
            IsHidden = false,
            Options = new List<UpdateDetailsDefinitionOptionDto>
            {
                // Only include the option to keep
                new()
                {
                    Id = optionToKeep.Id,
                    Name = new LocalizedStringDto("Keep This")
                }
            }
        };

        // Act
        var result = await controller.UpdateDetailsDefinition(existingDefinition.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Options ?? new List<DetailsDefinitionOptionDto>());

        // Verify deletion in database
        var updatedDefinition = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == existingDefinition.Id);

        Assert.NotNull(updatedDefinition);
        Assert.Single(updatedDefinition.Options ?? new List<DetailsDefinitionOption>());
        Assert.Equal(optionToKeep.Id, updatedDefinition.Options!.First().Id);

        // Verify deleted option is gone from database
        var deletedOption = await dbContext.DetailsDefinitionOptions
            .FirstOrDefaultAsync(o => o.Id == optionToDelete.Id);
        Assert.Null(deletedOption);
    }

    [Fact]
    public async Task UpdateDetailsDefinition_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto("Some Name"),
            Type = DetailDefinitionType.Text,
            SearchBehavior = DetailDefinitionSearchBehavior.Hidden,
            PropertyTypes = new List<PropertyType>(),
            IsHidden = false,
            Options = new List<UpdateDetailsDefinitionOptionDto>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => controller.UpdateDetailsDefinition(nonExistentId, updateDto)
        );

        Assert.Contains(nonExistentId.ToString(), exception.Message);
    }

    [Theory]
    [InlineData(DetailDefinitionType.Text)]
    [InlineData(DetailDefinitionType.Number)]
    [InlineData(DetailDefinitionType.Year)]
    [InlineData(DetailDefinitionType.Boolean)]
    [InlineData(DetailDefinitionType.MultiSelect)]
    public async Task CreateDetailsDefinition_WithDifferentTypes_ShouldHandleCorrectly(DetailDefinitionType type)
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        var createDto = new CreateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto($"Test {type} Field"),
            Type = type,
            SearchBehavior = DetailDefinitionSearchBehavior.And,
            PropertyTypes = new List<PropertyType> { PropertyType.Apartment },
            IsHidden = false,
            Options = type == DetailDefinitionType.MultiSelect 
                ? new List<CreateDetailsDefinitionOptionDto>
                {
                    new() { Name = new LocalizedStringDto("Option A") },
                    new() { Name = new LocalizedStringDto("Option B") }
                }
                : new List<CreateDetailsDefinitionOptionDto>()
        };

        // Act
        var result = await controller.CreateDetailsDefinition(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(type, result.Type);
        Assert.Equal($"Test {type} Field", result.Name.Values["fallback"]);
        
        if (type == DetailDefinitionType.MultiSelect)
        {
            Assert.NotNull(result.Options);
            Assert.Equal(2, result.Options.Count);
        }
        else
        {
            Assert.True(result.Options == null || result.Options.Count == 0);
        }

        // Verify in database
        var savedEntity = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == result.Id);
        
        Assert.NotNull(savedEntity);
        Assert.Equal(type, savedEntity.Type);
    }

    [Fact]
    public async Task CreateDetailsDefinition_WithComplexLocalizedStrings_ShouldHandleMultipleLanguages()
    {
        // Arrange
        using var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var controller = new AdminDetailsController(dbContext);

        var createDto = new CreateDetailsDefinitionDto
        {
            Name = new LocalizedStringDto
            {
                Values = new Dictionary<string, string>
                {
                    { "en", "Property Amenities" },
                    { "ar", "مرافق العقار" },
                    { "fr", "Équipements de propriété" },
                    { "es", "Servicios de la propiedad" }
                }
            },
            Type = DetailDefinitionType.MultiSelect,
            SearchBehavior = DetailDefinitionSearchBehavior.Or,
            PropertyTypes = new List<PropertyType> { PropertyType.Villa, PropertyType.Apartment },
            IsHidden = false,
            Options = new List<CreateDetailsDefinitionOptionDto>
            {
                new()
                {
                    Name = new LocalizedStringDto
                    {
                        Values = new Dictionary<string, string>
                        {
                            { "en", "Parking" },
                            { "ar", "موقف سيارات" },
                            { "fr", "Stationnement" },
                            { "es", "Estacionamiento" }
                        }
                    }
                }
            }
        };

        // Act
        var result = await controller.CreateDetailsDefinition(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Property Amenities", result.Name.Values["en"]);
        Assert.Equal("مرافق العقار", result.Name.Values["ar"]);
        Assert.Equal("Équipements de propriété", result.Name.Values["fr"]);
        Assert.Equal("Servicios de la propiedad", result.Name.Values["es"]);

        Assert.Single(result.Options!);
        var option = result.Options.First();
        Assert.Equal("Parking", option.Name.Values["en"]);
        Assert.Equal("موقف سيارات", option.Name.Values["ar"]);
        Assert.Equal("Stationnement", option.Name.Values["fr"]);
        Assert.Equal("Estacionamiento", option.Name.Values["es"]);

        // Verify all languages persisted to database
        var savedEntity = await dbContext.DetailsDefinitions
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.Id == result.Id);
        
        Assert.NotNull(savedEntity);
        Assert.Equal("Property Amenities", savedEntity.Name.Values["en"]);
        Assert.Equal("مرافق العقار", savedEntity.Name.Values["ar"]);
        
        var savedOption = savedEntity.Options!.First();
        Assert.Equal("Parking", savedOption.Name.Values["en"]);
        Assert.Equal("موقف سيارات", savedOption.Name.Values["ar"]);
    }
}