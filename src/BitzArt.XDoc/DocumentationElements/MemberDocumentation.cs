﻿using System.Reflection;
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
        if (target is null)
        {
            return null;
        }

        return Source.Get(target);
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
        : base(source, field, node) { }
}

/// <inheritdoc/>
public sealed class MethodDocumentation : MemberDocumentation<MethodInfo>
{
    internal MethodDocumentation(XDoc source, MethodInfo method, XmlNode? node)
        : base(source, method, node) { }
}

/// <inheritdoc/>
public sealed class PropertyDocumentation : MemberDocumentation<PropertyInfo>
{
    internal PropertyDocumentation(XDoc source, PropertyInfo property, XmlNode? node)
        : base(source, property, node) { }
}