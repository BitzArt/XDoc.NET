using System.Reflection;

namespace BitzArt.XDoc;

/// <summary>
/// Provides extension methods for the <see cref="Assembly"/> class.
/// </summary>
internal static class AssemblyExtensions
{
    /// <summary>
    /// Gets the file path to the XML documentation file associated with the specified <see cref="Assembly"/>.
    /// </summary>
    /// <param name="assembly">The assembly for which to retrieve the XML documentation file path.</param>
    /// <returns>
    /// The full file path to the XML documentation file for the given assembly.
    /// </returns>
    internal static string GetXmlDocumentationFilePath(this Assembly assembly)
    {
        // Note: At runtime, .NET loads assemblies from the output directory.
        // But during EF Core migrations, assemblies may be loaded from the NuGet cache.
        // This method always looks for the XML doc in the output folder (AppContext.BaseDirectory),
        // so ensure the XML file is present there for both runtime and migrations.
        
        var fileName = Path.GetFileName(assembly.Location);
        var xmlDocumentationFilePath = Path.ChangeExtension(Path.Combine(AppContext.BaseDirectory, fileName), "xml");

        return xmlDocumentationFilePath;
    }
}