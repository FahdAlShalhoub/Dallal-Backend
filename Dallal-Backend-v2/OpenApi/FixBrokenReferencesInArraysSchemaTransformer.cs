using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Dallal_Backend_v2.OpenApi;

public sealed class FixBrokenReferencesInArraysSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(schema);

        if (schema.Properties is null)
        {
            return Task.CompletedTask;
        }

        var propertiesToFix = schema
            .Properties.Select(x => x.Value)
            .Where(x => x.Type == "array")
            .Where(x => x.Items is not null)
            .Where(x => !string.IsNullOrEmpty(x.Items.Reference?.Id))
            .Where(x => x.Items.Annotations is not null);

        foreach (var property in propertiesToFix)
        {
            if (!property.Items.Annotations.TryGetValue("x-schema-id", out var schemaId))
            {
                continue;
            }

            property.Items.Reference = new OpenApiReference
            {
                Type = ReferenceType.Schema,
                Id = schemaId.ToString(),
            };
        }

        return Task.CompletedTask;
    }
}
