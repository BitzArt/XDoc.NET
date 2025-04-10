using System.Xml;

namespace BitzArt.XDoc;

/// <summary>
/// A base class for <see cref="XDoc"/> documentation elements.
/// </summary>
public abstract class DocumentationElement : IDocumentationElement
{
    /// <summary>
    /// XML documentation node.
    /// </summary>
    public XmlNode? Node { get; private init; }

    /// <summary>
    /// Source <see cref="XDoc"/> instance used to generate this <see cref="DocumentationElement"/>.
    /// </summary>
    public XDoc Source { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentationElement"/> class.
    /// </summary>
    /// <param name="source">The source of the documentation.</param>
    /// <param name="node">The XML node that contains the documentation.</param>
    internal DocumentationElement(XDoc source, XmlNode? node)
    {
        Source = source;
        Node = node;
    }

    /// <summary>
    /// Gets the text content of the XML node.
    /// </summary>
    public string Text => Node?.InnerText.Trim() ?? string.Empty;
}