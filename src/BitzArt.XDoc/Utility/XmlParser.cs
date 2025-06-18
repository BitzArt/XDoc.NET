using System.Reflection;
using System.Xml;

namespace BitzArt.XDoc;

internal class XmlParser
{
    private readonly XDoc _source;
    private readonly Assembly _assembly;
    private readonly XmlDocument _xml;
    private readonly Dictionary<Type, TypeDocumentation> _results;

    /// <summary>
    /// Parses an XML documentation file and creates a dictionary mapping types to their documentation.
    /// </summary>
    /// <param name="source">The XDoc instance that initiated the parsing</param>
    /// <param name="assembly">The assembly containing the types to be documented</param>
    /// <param name="xml">The XML file containing the auto-generated documentation</param>
    /// <returns>A dictionary mapping types to their TypeDocumentation objects</returns>
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
        {
            throw new InvalidOperationException("Invalid XML node.");
        }

        var name = (node.Attributes["name"]?.Value);

        if (name == null)
        {
            throw new InvalidOperationException($"No 'name' attribute found in XML node '{node.Value}'.");
        }

        switch (name[0])
        {
            case 'T': ParseTypeNode(node, name[2..]); break; //Types start with 'T'
            case 'P': ParsePropertyNode(node, name[2..]); break; //Properties start with 'P'
            case 'F': ParseFieldNode(node, name[2..]); break; //Fields start with 'F'
            case 'M': ParseMethodNode(node, name[2..]); break; //Methods and ctors start with 'M'
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
            (type, memberName, _) => type.GetProperty(memberName),
            member => new PropertyDocumentation(_source, member, node));

    private FieldDocumentation ParseFieldNode(XmlNode node, string name)
        => ParseMemberNode(name,
            (type, memberName, _) => type.GetField(memberName),
            member => new FieldDocumentation(_source, member, node));

    private MethodDocumentation? ParseMethodNode(XmlNode node, string name)
        => ParseMemberNode(name,
            (type, memberName, parameters) => GetMethodOrConstructor(type, memberName, parameters),
            member => new MethodDocumentation(_source, member, node));

    private static MethodBase? GetMethodOrConstructor(Type type, string name, IReadOnlyCollection<string> parameters)
        => name switch
        {
            "#ctor" => GetConstructor(type, parameters),
            "#cctor" => GetConstructor(type, parameters, isStatic: true),
            _ => GetMethod(type, name, parameters)
        };

    private static MethodInfo? GetMethod(Type ownerType, string name, IReadOnlyCollection<string> parameters)
    {
        var methods = ownerType.GetMethods();

        var method = methods
            .Where(method => method.Name == name)
            .SingleOrDefault(method =>
            {
                var actualParameters = method.GetParameters();

                return HasMatchingParameterTypes(actualParameters, parameters);
            });

        return method;
    }

    private static ConstructorInfo? GetConstructor(Type ownerType, IReadOnlyCollection<string> parameters, bool isStatic = false)
    {
        var bindingFlags = isStatic
            ? BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            : BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var constructors = ownerType.GetConstructors(bindingFlags);

        var constructor = constructors.SingleOrDefault(ctor =>
        {
            var actualParameters = ctor.GetParameters();

            return HasMatchingParameterTypes(actualParameters, parameters);
        });

        return constructor;
    }

    /// <summary>
    /// Determines if a method or constructor's parameter types match the expected parameter type names.
    /// </summary>
    /// <param name="actualParameters">
    /// Collection of actual parameter information from the method or constructor.
    /// </param>
    /// <param name="expectedParameters">Collection of expected parameter type names.</param>
    /// <returns>True if the parameter signatures match; otherwise, false.</returns>
    private static bool HasMatchingParameterTypes(
        ParameterInfo[] actualParameters,
        IReadOnlyCollection<string> expectedParameters)
    {
        if (actualParameters.Length == 0 && expectedParameters.Count == 0)
        {
            return true;
        }

        if (actualParameters.Length != expectedParameters.Count)
        {
            return false;
        }

        var parameters = actualParameters
            .Select(p => GetTypeFriendlyName(p.ParameterType))
            .ToList();

        return parameters.All(expectedParameters.Contains);
    }

    private TDocumentation ParseMemberNode<TMember, TDocumentation>(
        string name,
        Func<Type, string, IReadOnlyCollection<string>, TMember?> getMember,
        Func<TMember, TDocumentation> getDocumentation)
        where TMember : MemberInfo
        where TDocumentation : MemberDocumentation<TMember>
    {
        var (typeName, memberName, parameters) = XmlMemberNameResolver.ResolveMemberSignature(name);

        var type = _assembly.GetType(typeName)
            ?? throw new InvalidOperationException($"Type '{typeName}' not found.");

        var memberInfo = getMember.Invoke(type, memberName, parameters)
            ?? throw new InvalidOperationException($"Member '{memberName}' not found in type '{type.Name}'.");

        var typeDocumentation = ResolveTypeDocumentation(type);

        var memberDocumentation = getDocumentation.Invoke(memberInfo);

        typeDocumentation.AddMemberData(memberInfo, memberDocumentation);

        return memberDocumentation;
    }

    /// <summary>
    /// Converts a Type object to a friendly readable string representation.
    /// Handles generic types by recursively formatting their type arguments.
    /// </summary>
    /// <param name="type">The Type object to convert to a string representation.</param>
    /// <returns>
    /// A string representation of the type. For non-generic types, returns the full name.
    /// For generic types, returns the type name followed by generic arguments in curly braces
    /// (e.g., "System.Collections.Generic.List{System.String}").
    /// </returns>
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
        if (IsGeneric(typeName))
        {
            var indexOfStartGenericParameter = typeName!.IndexOf('`');

            typeName = typeName[..indexOfStartGenericParameter];
        }

        // Get the generic arguments and format them recursively
        var genericArgs = string.Join(",", type.GetGenericArguments().Select(GetTypeFriendlyName));

        return typeName + '{' + genericArgs + '}';
    }

    private static bool IsGeneric(string? typeName) => !string.IsNullOrWhiteSpace(typeName) && typeName.Contains('`');

    /// <summary>
    /// Finds the type documentation for the given type if already exists;
    /// otherwise, creates a new one and adds it to the results.
    /// </summary>
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