using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BitzArt.XDoc;

/// <summary>
/// Extensions for configuring XML documentation comments for Entity Framework Core entities.
/// </summary>
[PublicAPI]
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures XML documentation comments for Entity Framework Core entities and their properties.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="xDoc"></param>
    public static ModelBuilder ConfigureComments(this ModelBuilder modelBuilder, XDoc xDoc)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            entityType.ConfigureEntityXmlDocumentation(xDoc);

            var properties = entityType.GetProperties();

            foreach (var property in properties)
            {
                entityType.ConfigurePropertyXmlDocumentation(xDoc, property);
            }
        }

        return modelBuilder;
    }
}