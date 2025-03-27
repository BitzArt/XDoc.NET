using System.Reflection;

namespace BitzArt.XDoc;

/// <summary>
/// Holds information about the documentation of an <see cref="System.Reflection.Assembly"/>.
/// </summary>
public sealed class AssemblyDocumentation : DocumentationElement, IDocumentationElement<Assembly>
{
    /// <summary>
    /// The <see cref="System.Reflection.Assembly"/> this documentation is provided for.
    /// </summary>
    public Assembly Assembly { get; private init; }

    Assembly IDocumentationElement<Assembly>.Target => Assembly;

    /// <summary>
    /// Documentation found for types in this <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    private readonly Dictionary<Type, TypeDocumentation> _typeData;

    internal AssemblyDocumentation(XDoc source, Assembly assembly)
        : this(source, assembly, XmlUtility.Fetch(source, assembly)) { }

    internal AssemblyDocumentation(XDoc source, Assembly assembly, Dictionary<Type, TypeDocumentation> typeData) : base(source, null)
    {
        Assembly = assembly;
        _typeData = typeData;
    }

    /// <summary>
    /// Gets the documentation for a <see cref="Type"/> defined in this assembly.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to retrieve documentation for.</param>
    /// <returns><see cref="TypeDocumentation"/> for the specified <see cref="Type"/> if available; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provided <see cref="Type"/> is not defined in this assembly.</exception>
    public TypeDocumentation? GetDocumentation(Type type)
    {
        if (type.Assembly != Assembly)
        {
            throw new InvalidOperationException("The provided type is not defined in this assembly.");
        }

        return _typeData.TryGetValue(type, out var result)
                ? result
                : null;
    }

    /// <inheritdoc/>
    public override string ToString() => $"Documentation for Assembly '{Assembly.GetName().Name}'";
}