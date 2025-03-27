using System.Xml;

namespace BitzArt.XDoc;

/// <summary>
/// Contains documentation for a specific target.
/// </summary>
/// <typeparam name="TTarget">Documentation target type.</typeparam>
public interface IDocumentationElement<TTarget> : IDocumentationElement
    where TTarget : class
{
    /// <summary>
    /// Target of this documentation.
    /// </summary>
    public TTarget Target { get; }
}

/// <inheritdoc cref="IDocumentationElement{TTarget}"/>
public interface IDocumentationElement
{
    /// <summary>
    /// XML documentation node, if available.
    /// </summary>
    public XmlNode? Node { get; }
}
