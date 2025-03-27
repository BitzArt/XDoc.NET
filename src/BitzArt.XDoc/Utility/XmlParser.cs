using System.Reflection;
using System.Xml;

namespace BitzArt.XDoc;

internal class XmlParser
{
    private readonly XDoc _source;
    private readonly Assembly _assembly;
    private readonly XmlDocument _xml;
    private readonly Dictionary<Type, TypeDocumentation> _results;

    public static Dictionary<Type, TypeDocumentation> Parse(XDoc source, Assembly assembly, XmlDocument xml)
    {
        var parser = new XmlParser(source, assembly, xml);
        parser.Parse();

        return parser._results;
    }

    internal XmlParser(XDoc source, Assembly assembly, XmlDocument xml)
    {
        _source = source;
        _assembly = assembly;
        _xml = xml;

        _results = [];
    }

    internal Dictionary<Type, TypeDocumentation> Parse()
    {
        var nodeList = _xml.SelectNodes("/doc/members/member")
            ?? throw new InvalidOperationException("Invalid XML.");

        foreach (XmlNode node in nodeList) Parse(node);

        return _results;
    }

    private void Parse(XmlNode node)
    {
        if (node.Attributes is null || node.Attributes.Count == 0)
            throw new InvalidOperationException("Invalid XML node.");

        var name = (node.Attributes["name"]?.Value)
            ?? throw new InvalidOperationException($"No 'name' attribute found in XML node '{node.Value}'.");

        switch (name[0])
        {
            case 'T': ParseTypeNode(node, name[2..]); break;
            case 'P': ParsePropertyNode(node, name[2..]); break;
            case 'F': ParseFieldNode(node, name[2..]); break;
            case 'M': ParseMethodNode(node, name[2..]); break;
        }
    }

    private TypeDocumentation ParseTypeNode(XmlNode node, string name)
    {
        var type = _assembly.GetType(name)
            ?? throw new InvalidOperationException($"Type '{name}' not found.");

        // We could handle this case by finding and updating the existing object,
        // but I don't see a reason why this would be necessary.
        if (_results.ContainsKey(type))
        {
            throw new InvalidOperationException("Invalid XML. Type nodes should always go first in the XML file");
        }

        var typeDocumentation = new TypeDocumentation(_source, type, node);

        _results[type] = typeDocumentation;

        return typeDocumentation;
    }

    private PropertyDocumentation ParsePropertyNode(XmlNode node, string name)
    {
        var (type, memberName) = ResolveTypeAndMemberName(name);

        var propertyInfo = type.GetProperty(memberName)
                           ?? throw new InvalidOperationException(
                               $"Property '{memberName}' not found in type '{type.Name}'.");

        var typeDocumentation = ResolveOwnerType(type);

        var propertyDocumentation = new PropertyDocumentation(_source, propertyInfo, node);

        typeDocumentation.AddMemberData(propertyInfo, propertyDocumentation);

        return propertyDocumentation;
    }

    private FieldDocumentation ParseFieldNode(XmlNode node, string name)
    {
        var (type, memberName) = ResolveTypeAndMemberName(name);

        var fieldInfo = type.GetField(memberName)
                        ?? throw new InvalidOperationException(
                            $"Field '{memberName}' not found in type '{type.Name}'.");

        if (fieldInfo is null)
        {
            throw new InvalidOperationException($"Field '{memberName}' not found in type '{type.Name}'.");
        }

        var typeDocumentation = ResolveOwnerType(type);

        var fieldDocumentation = new FieldDocumentation(_source, fieldInfo, node);

        typeDocumentation.AddMemberData(fieldInfo, fieldDocumentation);

        return fieldDocumentation;
    }

    private MethodDocumentation? ParseMethodNode(XmlNode node, string name)
    {
        var (type, memberName) = ResolveTypeAndMemberName(name);
        var parameters = GetMethodParameters(name);
        var typeMethods = type.GetMethods();

        var methodInfo = typeMethods
            .Where(method => method.Name == memberName)
            .Where(method =>
            {
                var methodParameters = method
                    .GetParameters()
                    .Select(o => GetTypeFriendlyName(o.ParameterType))
                    .ToList();

                if (methodParameters.Count != parameters.Count)
                {
                    return false;
                }

                if (methodParameters.All(mp => parameters.Contains(mp)))
                {
                    return true;
                }

                return false;
            })
            .SingleOrDefault();

        if (methodInfo is null)
        {
            return null;
        }

        var typeDocumentation = ResolveOwnerType(type);

        var methodDocumentation = new MethodDocumentation(_source, methodInfo, node);

        typeDocumentation.AddMemberData(methodInfo, methodDocumentation);

        return methodDocumentation;
    }

    /// <summary>
    /// Get the friendly name of a type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static string GetTypeFriendlyName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.FullName ?? "";
        }

        // Get the name of the generic type definition (e.g. System.Nullable`1)
        var genericTypeDefinition = type.GetGenericTypeDefinition();
        var typeName = genericTypeDefinition.FullName;

        // Remove the `1 from the generic type name
        if (!string.IsNullOrWhiteSpace(typeName))
        {
            if (typeName.Contains('`'))
            {
                typeName = typeName[..typeName.IndexOf('`')];
            }
        }

        // Get the generic arguments and format them recursively
        var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeFriendlyName));

        var friendlyName = $"{typeName}{{{genericArgs}}}";

        return friendlyName;
    }

    /// <summary>
    /// Get the parameters of a method from the XML documentation.
    /// </summary>
    /// <param name="xmlMember"></param>
    /// <returns></returns>
    private static List<string> GetMethodParameters(string xmlMember)
    {
        var start = xmlMember.IndexOf('(');
        var end = xmlMember.LastIndexOf(')');

        if (start == -1 || end == -1 || end <= start)
        {
            return [];
        }

        // Extract the substring that contains the parameters.
        var paramList = xmlMember.Substring(start + 1, end - start - 1);

        if (string.IsNullOrWhiteSpace(paramList))
        {
            return [];
        }

        // Split the parameter list by commas and trim each parameter.
        var methodParameters = paramList.Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Select(p => p.Replace("`0", ""))
            .Select(p => p.Replace("`", ""))
            .ToList();

        return methodParameters;
    }

    private (Type type, string memberName) ResolveTypeAndMemberName(string name)
    {
        if (name.Contains('`'))
        {
            name = name[..name.IndexOf('`')];
        }

        if (name.Contains('('))
        {
            name = name[..name.IndexOf('(')];
        }

        var index = name.LastIndexOf('.');

        if (index == -1)
        {
            throw new InvalidOperationException("Encountered invalid XML node.");
        }

        var (typeName, memberName) = (name[..index], name[(index + 1)..]);

        var type = _assembly.GetType(typeName)
                   ?? throw new InvalidOperationException($"Type '{typeName}' not found.");

        return (type, memberName);
    }

    private TypeDocumentation ResolveOwnerType(Type type)
    {
        if (_results.TryGetValue(type, out var result))
        {
            return result;
        }

        result = new TypeDocumentation(_source, type, node: null);

        _results.Add(type, result);

        return result;
    }
}