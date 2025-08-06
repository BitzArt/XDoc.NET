using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;

namespace BitzArt.XDoc.PlainText;

/// <summary>
/// Lightweight renderer that converts XML documentation to plain text.
/// This implementation can only render the text content of the XML nodes but not resolve and render references.
/// </summary>
public class PlainTextRenderer
{
    private readonly RendererOptions _options;

    internal PlainTextRenderer() : this(new RendererOptions())
    {
    }

    internal PlainTextRenderer(RendererOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Renders the provided documentation element's own XML node content to plain text.
    /// </summary>
    /// <returns>A string containing rendered plain text.</returns>
    public string Render(IMemberDocumentation documentation)
    {
        ArgumentNullException.ThrowIfNull(documentation);

        return Normalize(RenderNode(documentation, documentation.Node, documentation.Member));
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

    private string RenderNode(IMemberDocumentation documentation, XmlNode? node, MemberInfo? target) => node switch
    {
        null => string.Empty,
        XmlText textNode => RenderTextNode(textNode),
        XmlElement element => RenderXmlElement(documentation, element, target),
        _ => node.InnerText
    };

    /// <summary>
    /// Renders the content of an XML element to plain text, including handling child nodes and references.
    /// </summary>
    /// <param name="documentation">The documentation element to render.</param>
    /// <param name="element">The XML element to render.</param>
    /// <param name="target"></param>
    /// <returns>The plain text representation of the XML element.</returns>
    private string RenderXmlElement(IMemberDocumentation documentation, XmlElement element, MemberInfo? target)
    {
        // Reference
        if (element.Attributes["cref"] is not null)
        {
            return RenderReference(element);
        }

        // Direct inheritance
        if (element.Name == "inheritdoc")
        {
            return RenderDirectInheritance(documentation);
        }

        var builder = new StringBuilder();

        foreach (XmlNode child in element.ChildNodes)
        {
            builder.Append(RenderNode(documentation, child, target));
        }

        return builder.ToString();
    }

    private string RenderReference(XmlElement element)
    {
        var cref = MemberReferenceInfo.FromReferenceString(element.Attributes["cref"]?.Value!);

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

    private string RenderDirectInheritance(IMemberDocumentation documentation)
    {
        var targetDocumentation = documentation.GetInheritanceTargetDocumentation();

        if (targetDocumentation is null)
        {
            return string.Empty;
        }

        return Render(targetDocumentation);
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