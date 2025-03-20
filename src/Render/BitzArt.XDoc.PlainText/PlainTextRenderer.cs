using System.Reflection;
using System.Text;
using System.Xml;

namespace BitzArt.XDoc;

/// <summary>
/// Lightweight XML renderer that converts XML documentation to plain text.
/// This implementation, can only render the text content of the XML nodes, but not resolve and render references.
/// </summary>
public class PlainTextRenderer
{
    /// <summary>
    /// Converts an XML documentation node to the plain text.
    /// </summary>
    /// <param name="documentation"></param>
    /// <param name="forceSingleLine"></param>
    /// <param name="useShortTypeNames"></param>
    /// <returns></returns>
    public static string Render(
        MemberDocumentation? documentation,
        bool forceSingleLine = false,
        bool useShortTypeNames = true)
    {
        if (documentation == null)
        {
            return string.Empty;
        }

        return new PlainTextRenderer(documentation, new RendererOptions
        {
            ForceSingleLine = forceSingleLine,
            UseShortTypeNames = useShortTypeNames
        }).Render();
    }

    /// <summary>
    /// The documentation instance to be rendered by this renderer.
    /// This field is initialized in the constructor and remains readonly afterward.
    /// </summary>
    private readonly MemberDocumentation _documentation;

    private readonly RendererOptions _options;

    private PlainTextRenderer(MemberDocumentation documentation, RendererOptions rendererOptions)
    {
        _options = rendererOptions;
        _documentation = documentation;
    }

    /// <summary>
    /// Renders the current documentation to plain text.
    /// </summary>
    /// <returns>A normalized plain text representation of the documentation.</returns>
    private string Render() => Normalize(Render(_documentation.Node));

    /// <summary>
    /// Renders the content of an XML node to plain text.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private string Render(XmlNode? node) => node switch
    {
        null => string.Empty,
        XmlText textNode => RenderTextNode(textNode),
        XmlElement element => RenderXmlElement(element),
        _ => node.InnerText
    };

    /// <summary>
    /// Normalize the input string by removing extra empty lines and trimming each line.
    /// </summary>
    private string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var lines = input
            .Split('\n')
            .Select(line => line.Trim())
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToList();

        var separator = _options.ForceSingleLine ? " " : "\n";

        return string.Join(separator, lines);
    }

    /// <summary>
    /// Renders the content of an XML element to plain text, including handling child nodes and references.
    /// </summary>
    /// <param name="element">The XML element to render.</param>
    /// <returns>The plain text representation of the XML element.</returns>
    private string RenderXmlElement(XmlElement element)
    {
        var builder = new StringBuilder();

        if (element.Attributes["cref"] != null || element.Name == "inheritdoc")
        {
            return RenderReference(element);
        }

        foreach (XmlNode child in element.ChildNodes)
        {
            builder.Append(Render(child));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Render a see/seealso reference.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private string RenderReference(XmlElement element)
    {
        var documentationReference = _documentation.GetReference(element);

        if (documentationReference == null)
        {
            return string.Empty;
        }

        if (documentationReference.Target != null)
        {
            return RenderDocumentationReference(documentationReference);
        }
        else if (documentationReference.Cref != null)
        {
            return RenderSimpleDocumentationReference(documentationReference);
        }

        return string.Empty;
    }

    private string RenderSimpleDocumentationReference(DocumentationReference documentationReference)
    {
        var cref = documentationReference.Cref;

        if (cref is null)
        {
            return string.Empty;
        }

        if (cref.Prefix is "T:")
        {
            if (_options.UseShortTypeNames)
            {
                return cref.ShortType;
            }

            return cref.Type;
        }

        if (cref.Prefix is "M:" or "P:" or "F:")
        {
            if (_options.UseShortTypeNames)
            {
                return $"{cref.ShortType}.{cref.Member}";
            }

            return $"{cref.Type}.{cref.Member}";
        }

        return string.Empty;
    }

    private string RenderDocumentationReference(DocumentationReference documentationReference)
    {
        if (documentationReference.Target == null)
        {
            return string.Empty;
        }

        if (documentationReference.RequirementNode.Name == "inheritdoc")
        {
            return Render(documentationReference.Target);
        }

        var text = documentationReference.Target switch
        {
            TypeDocumentation typeDocumentation => typeDocumentation.Type.Name,
            FieldDocumentation fieldDocumentation => RenderMember(fieldDocumentation),
            PropertyDocumentation propertyDocumentation => RenderMember(propertyDocumentation),
            MethodDocumentation methodDocumentation => RenderMember(methodDocumentation),
            _ => string.Empty
        };

        return text;
    }

    private string RenderMember<T>(TypeMemberDocumentation<T> documentation)
        where T : MemberInfo
    {
        if (_options.UseShortTypeNames)
        {
            return $"{documentation.DeclaringType.Name}.{documentation.MemberName}";
        }

        return $"{documentation.DeclaringType.FullName}.{documentation.MemberName}";
    }

    /// <summary>
    /// Renders the content of an XML text node to plain text.
    /// </summary>
    /// <param name="textNode">The XML text node to render.</param>
    /// <returns>The plain text representation of the XML text node.</returns>
    private string RenderTextNode(XmlText textNode)
    {
        return textNode.Value ?? string.Empty;
    }
}