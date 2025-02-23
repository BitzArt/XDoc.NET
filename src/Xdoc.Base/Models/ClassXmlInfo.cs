using System.Collections.Frozen;
using System.Xml;
using Xdoc.Abstractions;

namespace Xdoc.Models;

/// <summary>
/// Represents a class in the XML documentation.
/// </summary>
public record ClassXmlInfo : IClassXmlInfo
{
    private readonly Type _type;
    private XmlSummary _summary;

    /// <summary>
    /// Documented properties of the class.
    /// </summary>
    public IReadOnlyDictionary<string, IPropertyXmlInfo> Properties { get; }

    /// <summary>
    /// Class name.
    /// </summary>
    public string Name => _type.FullName!;

    /// <summary>
    /// Assembly which the class belongs to.
    /// </summary>
    public IAssemblyXmlInfo Assembly { get; }

    /// <summary>
    /// Class summary.
    /// </summary>
    public IXmlSummary Summary { get; }

    // public ClassXmlInfo? Parent => Assembly.DocumentStore.GetClassInfo(_type.BaseType);

    /// <summary>
    /// Initialize a new instance of <see cref="ClassXmlInfo"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="assembly"></param>
    /// <param name="documentation"></param>
    internal ClassXmlInfo(Type type, AssemblyXmlInfo assembly, IReadOnlyDictionary<string, XmlNode> documentation)
    {
        _type = type;

        Assembly = assembly;
        Summary = CreateSummary(type, documentation);
        Properties = CreateProperties(documentation);
    }

    /// <summary>
    /// Create a summary for a type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="documentation"></param>
    /// <returns></returns>
    private XmlSummary CreateSummary(Type type, IReadOnlyDictionary<string, XmlNode> documentation)
    {
        var typeNameKey = $"T:{type.FullName}";
        var xmlNode = documentation.GetValueOrDefault(typeNameKey);

        return new XmlSummary(xmlNode);
    }

    /// <summary>
    /// Create a dictionary of properties.
    /// </summary>
    /// <param name="documentation"></param>
    /// <returns></returns>
    private IReadOnlyDictionary<string, IPropertyXmlInfo> CreateProperties(
        IReadOnlyDictionary<string, XmlNode> documentation)
    {
        var typeNamePrefix = $"P:{_type.FullName}.";

        var propertyKeys = documentation.Keys
            .Where(k => k.StartsWith(typeNamePrefix))
            .ToFrozenSet();

        var result = new Dictionary<string, IPropertyXmlInfo>();

        foreach (var propertyKey in propertyKeys)
        {
            var propertyName = propertyKey[(propertyKey.LastIndexOf('.') + 1)..];

            var node = documentation[propertyKey];
            var propertyXmlInfo = new PropertyXmlInfo(propertyName, this, node);

            result.Add(propertyXmlInfo.Name, propertyXmlInfo);
        }

        return result.ToFrozenDictionary();
    }

    /// <summary>
    /// Get information about a property.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public IPropertyXmlInfo? GetPropertyInfo(string propertyName)
    {
        return Properties.GetValueOrDefault(propertyName);
    }

    /// <summary>
    /// Get a string representation of the of <see cref="ClassXmlInfo"/>.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}