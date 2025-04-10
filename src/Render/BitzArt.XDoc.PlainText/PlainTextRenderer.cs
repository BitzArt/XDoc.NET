using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;

namespace BitzArt.XDoc.PlainText;

/// <summary>
/// Lightweight renderer that converts XML documentation to plain text.
/// This implementation, can only render the text content of the XML nodes, but not resolve and render references.
/// </summary>
public class PlainTextRenderer
{
    private readonly XDoc _xdoc;
    private readonly RendererOptions _options;

    internal PlainTextRenderer(XDoc xdoc)
        : this(xdoc, new RendererOptions())
    {
    }

    internal PlainTextRenderer(XDoc xdoc, RendererOptions options)
    {
        _xdoc = xdoc;
        _options = options;
    }

    /// <summary>
    /// Renders the provided documentation element to a string.
    /// </summary>
    /// <param name="documentation"></param>
    /// <returns></returns>
    public string Render(IDocumentationElement? documentation)
    {
        if (documentation is null)
        {
            return string.Empty;
        }

        var target = (documentation is IMemberDocumentation memberDocumentation)
            ? memberDocumentation.Member
            : null;

        return Normalize(Render(documentation.Node, target));
    }

    /// <summary>
    /// Normalize the input string by removing extra empty lines and trimming each line.
    /// </summary>
    private static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var lines = input
            .Split('\n')
            .Select(line => line.Trim())
            .ToList();

        return string.Join("\n", lines).Trim();
    }

    private string Render(XmlNode? node, MemberInfo? target) => node switch
    {
        null => string.Empty,
        XmlText textNode => RenderTextNode(textNode),
        XmlElement element => RenderXmlElement(element, target),
        _ => node.InnerText
    };

    /// <summary>
    /// Renders the content of an XML element to plain text, including handling child nodes and references.
    /// </summary>
    /// <param name="element">The XML element to render.</param>
    /// <param name="target"></param>
    /// <returns>The plain text representation of the XML element.</returns>
    private string RenderXmlElement(XmlElement element, MemberInfo? target)
    {
        // Reference
        if (element.Attributes["cref"] is not null)
        {
            return RenderReference(element);
        }

        // Direct inheritance
        if (element.Name == "inheritdoc")
        {
            return RenderDirectInheritance(target);
        }

        var builder = new StringBuilder();

        foreach (XmlNode child in element.ChildNodes)
        {
            builder.Append(Render(child, target));
        }

        return builder.ToString();
    }

    private string RenderReference(XmlElement element)
    {
        MemberIdentifier.TryCreate(element.Attributes["cref"]?.Value!, out var cref);

        if (cref is null)
        {
            return string.Empty;
        }

        var type = _options.RemoveNamespace ? cref.ShortType : cref.Type;

        if (cref.IsMember)
        {
            return $"{type}.{cref.Member}";
        }
        
        if (cref.IsType)
        {
            return type;
        }

        throw new UnreachableException("Invalid cref format: the cref should be either a type or a member.");
    }

    private string RenderDirectInheritance(MemberInfo? target)
    {
        if (target is null)
        {
            return string.Empty;
        }

        var targetMember = InheritanceResolver.GetTargetMember(target);

        if (targetMember is null)
        {
            return string.Empty;
        }

        var documentationElement = _xdoc.Get(targetMember);

        if (documentationElement is null)
        {
            return RenderDirectInheritance(targetMember);
        }

        return Render(documentationElement);
    }

    /// <summary>
    /// Renders the content of an XML text node to plain text.
    /// </summary>
    /// <param name="textNode">The XML text node to render.</param>
    /// <returns>The plain text representation of the XML text node.</returns>
    private static string RenderTextNode(XmlText textNode)
    {
        return textNode.Value ?? string.Empty;
    }
}