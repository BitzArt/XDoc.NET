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
        => ParseMemberNode(name,
            (type, memberName) => type.GetProperty(memberName),
            member => new PropertyDocumentation(_source, member, node));

    private FieldDocumentation ParseFieldNode(XmlNode node, string name)
        => ParseMemberNode(name,
            (type, memberName) => type.GetField(memberName),
            member => new FieldDocumentation(_source, member, node));

    private MethodDocumentation? ParseMethodNode(XmlNode node, string name)
        => ParseMemberNode(name,
            (type, memberName) =>
            {
                var parameters = GetMethodParameters(memberName);

                return type.GetMethods()
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
            },
            member => new MethodDocumentation(_source, member, node));

    private TDocumentation ParseMemberNode<TMember, TDocumentation>(string name, Func<Type, string, TMember?> getMember, Func<TMember, TDocumentation> getDocumentation)
        where TMember : MemberInfo
        where TDocumentation : MemberDocumentation<TMember>
    {
        var (type, memberName) = ResolveTypeAndMemberName(name);

        var memberInfo = getMember.Invoke(type, memberName)
            ?? throw new InvalidOperationException(
                $"Member '{memberName}' not found in type '{type.Name}'.");

        var typeDocumentation = ResolveTypeDocumentation(type);

        var memberDocumentation = getDocumentation.Invoke(memberInfo);

        typeDocumentation.AddMemberData(memberInfo, memberDocumentation);

        return memberDocumentation;
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

    // Fetches the parameter type names of a method from the XML documentation.
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

    // finds the type documentation for the given type if already exists;
    // otherwise, creates a new one and adds it to the results.
    private TypeDocumentation ResolveTypeDocumentation(Type type)
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