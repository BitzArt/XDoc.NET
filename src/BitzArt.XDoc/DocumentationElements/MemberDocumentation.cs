using System.Reflection;
using System.Text;
using System.Xml;

namespace BitzArt.XDoc;

/// <summary>
/// Contains documentation of a specific declared <typeparamref name="TMemberInfo"/>.
/// </summary>
/// <typeparam name="TMemberInfo">Type of the member.</typeparam>
public abstract class MemberDocumentation<TMemberInfo> : DocumentationElement, IDocumentationElement<TMemberInfo>, IMemberDocumentation
    where TMemberInfo : MemberInfo
{
    /// <inheritdoc/>
    public IMemberDocumentation? GetInheritanceTargetDocumentation()
    {
        var target = GetInheritanceTarget();
        var memberDocumentation = target is not null ? Source.Get(target) : null;

        if (memberDocumentation is null)
        {
            WriteInheritanceTargetDocumentationDiagnosticMessage(target);
        }

        return memberDocumentation;
    }

    private void WriteInheritanceTargetDocumentationDiagnosticMessage(MemberInfo? target)
    {
        var declaringAssembly = Member.DeclaringType?.Assembly;
        var targetAssembly = target?.DeclaringType?.Assembly;
        var memberName = $"{Member.DeclaringType}.{Member.Name}";
        var parentMemberName = $"{target?.DeclaringType}.{target?.Name}";

        var sb = new StringBuilder();

        sb.Append("WARNING! ");
        sb.Append($"Inherited documentation for '{memberName}' from '{parentMemberName}' is missing. ");

        if (declaringAssembly?.FullName == targetAssembly?.FullName)
        {
            // Seems we have a local member without documentation.
            sb.Append("Check if the parent member is documented. ");
        }
        else
        {
            // The target member is in a different assembly
            var targetAssemblyXmlDocumentationFilePath = targetAssembly?.GetXmlDocumentationFilePath();

            if (!File.Exists(targetAssemblyXmlDocumentationFilePath))
            {
                sb.Append("Ensure that the referenced assembly has an XML documentation file. ");
                sb.Append("If it's a NuGet package, verify that <IncludeAssets> tag configured. ");
            }
            else
            {
                sb.Append("Check that the referenced assembly's XML documentation contains the target comment. ");
            }
        }

        sb.Append("See details: https://github.com/BitzArt/XDoc.NET/blob/main/README.md#inherited-documentation");

        ConsoleUtility.WriteLine(sb.ToString(), ConsoleColor.Magenta);
    }

    /// <inheritdoc/>
    public MemberInfo? GetInheritanceTarget() => InheritanceResolver.GetTargetMember(Member);

    /// <summary>
    /// The <typeparamref name="TMemberInfo"/> this documentation if provided for.
    /// </summary>
    public TMemberInfo Member { get; private init; }

    MemberInfo IMemberDocumentation.Member => Member;

    TMemberInfo IDocumentationElement<TMemberInfo>.Target => Member;

    /// <summary>
    /// XML documentation node.
    /// </summary>
    public virtual XmlNode? Node { get; private init; }

    internal MemberDocumentation(XDoc source, TMemberInfo member, XmlNode? node)
        : base(source)
    {
        Member = member;
        Node = node;
    }

    /// <inheritdoc/>
    public override string ToString() => $"Documentation for {typeof(TMemberInfo).Name} '{Member.Name}'";
}

/// <inheritdoc/>
public sealed class FieldDocumentation : MemberDocumentation<FieldInfo>
{
    internal FieldDocumentation(XDoc source, FieldInfo field, XmlNode? node)
        : base(source, field, node)
    {
    }
}

/// <inheritdoc/>
public sealed class MethodDocumentation : MemberDocumentation<MethodBase>
{
    internal MethodDocumentation(XDoc source, MethodBase method, XmlNode? node)
        : base(source, method, node)
    {
    }
}

/// <inheritdoc/>
public sealed class PropertyDocumentation : MemberDocumentation<PropertyInfo>
{
    internal PropertyDocumentation(XDoc source, PropertyInfo property, XmlNode? node)
        : base(source, property, node)
    {
    }
}