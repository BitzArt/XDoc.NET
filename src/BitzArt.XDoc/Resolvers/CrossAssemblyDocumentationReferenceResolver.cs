using System.Xml;
using JetBrains.Annotations;

namespace BitzArt.XDoc;

/// <summary>
/// Default implementation of <see cref="IDocumentationReferenceResolver"/> that
/// extracts references from XML documentation nodes.
/// </summary>
[PublicAPI]
public class CrossAssemblyDocumentationReferenceResolver : IDocumentationReferenceResolver
{
    /// <summary>
    /// Extracts a documentation reference from the provided XML node.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="node">The XML node to extract the reference from.</param>
    /// <returns>
    /// A <see cref="DocumentationReference"/> object if a reference can be extracted;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="NotImplementedException">Thrown when the node type is not supported.</exception>
    public DocumentationReference? GetReference(XDoc source, XmlNode node)
    {
        if (node.Attributes?["cref"] != null)
        {
            return GetCrefReference(source, node);
        }

        if (node.Name == "inheritdoc")
        {
            return GetInheritReference(source, node);
        }

        return null;
    }

    /// <summary>
    /// Processes an inheritdoc XML node to extract documentation reference.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="node">The inheritdoc XML node.</param>
    /// <returns>A documentation reference or null if reference cannot be extracted.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented yet.</exception>
    private DocumentationReference? GetInheritReference(XDoc source, XmlNode node)
    {
        //P:TestAssembly.B.Dog.Color

        var cref = Cref.TryCreate(node.ParentNode?.Attributes?["name"]?.Value, out var result) ? result : null;

        if (cref is null)
        {
            return null;
        }

        var type = GetType(cref.Type);
        var baseType = type.BaseType;

        if (baseType == null)
        {
            return null;
        }

        MemberDocumentation? targetDocumentation = null;

        if (cref.Prefix is "T:")
        {
            targetDocumentation = source.Get(baseType);
        }
        else if (cref.Prefix is "P:" or "M:" or "F:")
        {
            targetDocumentation = GetMemberDocumentation(source, baseType, cref.Member!);
        }

        if (targetDocumentation == null)
        {
            return null;
        }

        return new DocumentationReference(node, targetDocumentation, cref);
    }

    /// <summary>
    /// Processes an XML node with a cref attribute to extract documentation reference.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="node">The XML node containing the reference.</param>
    /// <returns>A documentation reference or null if reference cannot be extracted.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented yet.</exception>
    private DocumentationReference? GetCrefReference(XDoc source, XmlNode node)
    {
        var cref = Cref.TryCreate(node.Attributes?["cref"]?.Value, out var result) ? result : null;

        if (cref == null)
        {
            return null;
        }

        var type = GetType(cref.Type);

        MemberDocumentation? targetDocumentation = null;

        if (cref.Prefix is "T:")
        {
            targetDocumentation = source.Get(type);
        }
        else if (cref.Prefix is "P:" or "M:" or "F:")
        {
            targetDocumentation = GetMemberDocumentation(source, type, cref.Member!);
        }

        if (targetDocumentation == null)
        {
            return null;
        }

        return new DocumentationReference(node, targetDocumentation, cref);
    }

    private MemberDocumentation? GetMemberDocumentation(XDoc source, Type type, string memberName)
    {
        var memberInfos = type.GetMember(memberName);

        if (memberInfos.Length != 1)
        {
            return null;
        }

        var memberInfo = memberInfos.First();
        var memberDocumentation = source.Get(memberInfo);

        return memberDocumentation;
    }

    /// <summary>
    /// Resolves a Type object from its string name representation.
    /// </summary>
    /// <param name="name">The fully qualified name of the type to resolve.</param>
    /// <returns>The resolved Type object.</returns>
    /// <exception cref="TypeLoadException">Thrown when the specified type cannot be found.</exception>
    private static Type GetType(string name)
    {
        // First try direct type resolution
        var type = Type.GetType(name);

        if (type != null)
        {
            return type;
        }

        // If direct lookup fails, search through all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            type = assembly.GetType(name);

            if (type != null)
            {
                return type;
            }
        }

        throw new TypeLoadException($"Could not find type '{name}'.");
    }
}