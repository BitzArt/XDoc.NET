using System.Reflection;
using System.Xml;

namespace BitzArt.XDoc;

/// <summary>
/// Contains documentation for a specific value of type <typeparamref name="TTarget"/>.
/// </summary>
/// <typeparam name="TTarget">Type of the documentation owner.</typeparam>
public interface IDocumentationElement<TTarget> : IDocumentationElement
    where TTarget : class
{
    /// <summary>
    /// Target of this documentation.
    /// </summary>
    public TTarget Target { get; }
}

/// <summary>
/// Contains documentation for a specific declared member of the <see cref="Assembly"/>
/// </summary>
public interface IDocumentationElement
{
    /// <summary>
    /// XML documentation node, if available.
    /// </summary>
    public XmlNode? Node { get; }
}
