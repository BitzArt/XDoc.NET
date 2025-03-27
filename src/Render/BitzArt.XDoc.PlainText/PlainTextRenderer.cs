using System.Reflection;
using System.Text;
using System.Xml;

namespace BitzArt.XDoc;

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
    public string Render(DocumentationElement? documentation)
    {
        if (documentation == null)
        {
            return string.Empty;
        }

        var target = GetTarget(documentation);

        return Normalize(Render(documentation.Node, target));
    }

    private MemberInfo? GetTarget(DocumentationElement documentation)
    {
        if (documentation is IDocumentationElement<Type> typeDocumentation)
        {
            return typeDocumentation.Target;
        }

        if (documentation is IDocumentationElement<PropertyInfo> propertyDocumentation)
        {
            return propertyDocumentation.Target;
        }

        if (documentation is IDocumentationElement<MethodInfo> methodDocumentation)
        {
            return methodDocumentation.Target;
        }

        if (documentation is IDocumentationElement<FieldInfo> fieldDocumentation)
        {
            return fieldDocumentation.Target;
        }

        return null;
    }

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

    private string Render(XmlNode? node, MemberInfo? target) => node switch
    {
        null => string.Empty,
        XmlText textNode => RenderTextNode(textNode),
        XmlElement element => RenderXmlElement(element, target),
        _ => node.InnerText
    };

    /// <summary>
    /// Renders the content of an XML text node to plain text.
    /// </summary>
    /// <param name="textNode">The XML text node to render.</param>
    /// <returns>The plain text representation of the XML text node.</returns>
    private string RenderTextNode(XmlText textNode)
    {
        return textNode.Value ?? string.Empty;
    }

    /// <summary>
    /// Renders the content of an XML element to plain text, including handling child nodes and references.
    /// </summary>
    /// <param name="element">The XML element to render.</param>
    /// <param name="target"></param>
    /// <returns>The plain text representation of the XML element.</returns>
    private string RenderXmlElement(XmlElement element, MemberInfo? target)
    {
        if (element.Attributes["cref"] != null)
        {
            // Reference
            return RenderReference(element);
        }

        if (element.Name == "inheritdoc")
        {
            // Direct inheritance
            return RenderDirectInheritance(element, target);
        }

        var builder = new StringBuilder();

        foreach (XmlNode child in element.ChildNodes)
        {
            builder.Append(Render(child, null));
        }

        return builder.ToString();
    }

    private string RenderReference(XmlElement element)
    {
        var cref = new MemberIdentifier(element.Attributes["cref"]!.Value);

        var type = _options.UseShortTypeNames ? cref.ShortType : cref.Type;

        if (cref.IsMember)
        {
            return $"{type}.{cref.Member}";
        }

        return type;
    }

    private string RenderDirectInheritance(XmlElement element, MemberInfo? target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        if (target is Type type)
        {
            return GetTypeInheritedComment(type);
        }
        else if (target is PropertyInfo propertyInfo)
        {
            return GetMemberInheritedComment(propertyInfo);
        }
        else if (target is FieldInfo fieldInfo)
        {
            return GetMemberInheritedComment(fieldInfo);
        }
        else if (target is MethodInfo methodInfo)
        {
            return GetMemberInheritedComment(methodInfo);
        }

        return string.Empty;
    }

    private string GetMemberInheritedComment(MemberInfo memberInfo)
    {
        var type = memberInfo.DeclaringType ?? memberInfo.ReflectedType!;
        return GetMemberInheritedComment(memberInfo, type);
    }

    private string GetMemberInheritedComment(MemberInfo memberInfo, Type type)
    {
        // 1. Check Interfaces (own interfaces, not those inherited from base type)
        // 2. Enter Base Type
        // 3. Check own members
        // 4. Return to step 1
        
        var typeInterfaces = type.GetInterfaces();

        foreach (var interfaceType in typeInterfaces)
        {
            var memberWithSameMetadataDefinition = interfaceType.GetMemberWithSameMetadataDefinitionAs(memberInfo);

            var documentation = _xdoc.Get(memberWithSameMetadataDefinition);

            if (documentation != null)
            {
                return Render(documentation);
            }
        }

        // 2. Enter Base Type
        if (type.BaseType != null && type.BaseType != typeof(object))
        {
            var memberWithSameMetadataDefinition = type.BaseType.GetMemberWithSameMetadataDefinitionAs(memberInfo);
            
            // 3. Check own members (in this context, check base type's documentation)
            var documentation = _xdoc.Get(memberWithSameMetadataDefinition);

            if (documentation != null)
            {
                return Render(documentation);
            }

            // 4. Return to step 1 (recursively check the inheritance hierarchy)
            return GetMemberInheritedComment(memberInfo, type.BaseType);
        }

        return string.Empty;
    }

    private string GetTypeInheritedComment(Type type)
    {
        // 1. Check Interfaces (own interfaces, not those inherited from base type)
        // 2. Enter Base Type
        // 3. Check own members
        // 4. Return to step 1

        // 1. Check Interfaces (own interfaces, not those inherited from base type)
        // var baseInterfaces = type.BaseType?.GetInterfaces() ?? [];

        var typeInterfaces = type.GetInterfaces();

        foreach (var interfaceType in typeInterfaces)
        {
            var documentation = _xdoc.Get(interfaceType);

            if (documentation != null)
            {
                return Render(documentation);
            }
        }

        // 2. Enter Base Type
        if (type.BaseType != null && type.BaseType != typeof(object))
        {
            // 3. Check own members (in this context, check base type's documentation)
            var documentation = _xdoc.Get(type.BaseType);

            if (documentation != null)
            {
                return Render(documentation);
            }

            // 4. Return to step 1 (recursively check the inheritance hierarchy)
            return GetTypeInheritedComment(type.BaseType);
        }

        return string.Empty;
    }
}