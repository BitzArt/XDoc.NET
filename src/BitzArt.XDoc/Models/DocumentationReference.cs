using System.Xml;

namespace BitzArt.XDoc;

/// <summary>
/// Represents a class for documentation references.
/// </summary>
public class DocumentationReference
{
    /// <summary>
    /// Actual XML node that caused this requirement
    /// </summary>
    public XmlNode RequirementNode { get; private init; }

    /// <summary>
    /// The code reference that identifies the target element.
    /// </summary>
    public Cref? Cref { get; init; }

    /// <summary>
    /// Target member documentation
    /// </summary>
    public MemberDocumentation? Target { get; private init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="requirementNode"></param>
    /// <param name="target"></param>
    /// <param name="crefValue"></param>
    public DocumentationReference(XmlNode requirementNode, MemberDocumentation? target, string? crefValue)
    {
        Target = target;
        RequirementNode = requirementNode;
        Cref = Cref.TryCreate(crefValue, out var cref) ? cref : null;
    }
}