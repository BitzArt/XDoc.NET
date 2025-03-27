using System.Xml;

namespace BitzArt.XDoc;

/// <summary>
/// All documentation classes should inherit from this class.
/// This class contains code applicable to any member documentation (both Type and MemberInfo).
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
    protected DocumentationElement(XDoc source, XmlNode? node)
    {
        Source = source;
        Node = node;
    }
}