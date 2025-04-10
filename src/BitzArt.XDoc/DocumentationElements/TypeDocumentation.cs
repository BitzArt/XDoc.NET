using System.Reflection;
using System.Xml;

namespace BitzArt.XDoc;

/// <inheritdoc/>
public sealed class TypeDocumentation : MemberDocumentation<Type>, IDocumentationElement<Type>
{
    private readonly Dictionary<MemberInfo, DocumentationElement> _memberData;

    /// <summary>
    /// The Type this documentation if provided for.
    /// </summary>
    public Type Type => Member;

    /// <summary>
    /// List of documentation elements for all members declared within the scope of this <see cref="System.Type"/> that have documentation.
    /// </summary>
    public IReadOnlyDictionary<MemberInfo, DocumentationElement> MemberData => _memberData.AsReadOnly();

    internal TypeDocumentation(XDoc source, Type type, XmlNode? node)
        : base(source, type, node)
    {
        _memberData = [];
    }

    /// <summary>
    /// Gets the documentation for a <see cref="PropertyInfo"/> declared by this <see cref="Type"/>.
    /// </summary>
    /// <param name="property">The <see cref="PropertyInfo"/> to retrieve documentation for.</param>
    /// <returns>
    /// <see cref="PropertyDocumentation"/> for the specified <see cref="PropertyInfo"/> if it is available; otherwise, <see langword="null"/>.
    /// </returns>
    public PropertyDocumentation? GetDocumentation(PropertyInfo property)
        => GetDocumentation<PropertyDocumentation>(property);

    /// <summary>
    /// Gets the documentation for a <see cref="MethodInfo"/> declared by this <see cref="Type"/>.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo"/> to retrieve documentation for.</param>
    /// <returns>
    /// <see cref="MethodDocumentation"/> for the specified <see cref="MethodInfo"/> if it is available; otherwise, <see langword="null"/>.
    /// </returns>
    public MethodDocumentation? GetDocumentation(MethodInfo method)
        => GetDocumentation<MethodDocumentation>(method);

    /// <summary>
    /// Gets the documentation for a <see cref="FieldInfo"/> declared by this <see cref="Type"/>.
    /// </summary>
    /// <param name="field">The <see cref="FieldInfo"/> to retrieve documentation for.</param>
    /// <returns>
    /// <see cref="FieldDocumentation"/> for the specified <see cref="FieldInfo"/> if it is available; otherwise, <see langword="null"/>.
    /// </returns>
    public FieldDocumentation? GetDocumentation(FieldInfo field)
        => GetDocumentation<FieldDocumentation>(field);

    private TMemberDocumentationResult? GetDocumentation<TMemberDocumentationResult>(MemberInfo member)
        where TMemberDocumentationResult : DocumentationElement
    {
        return (TMemberDocumentationResult?)GetDocumentation(member);
    }
    
    internal DocumentationElement? GetDocumentation(MemberInfo member)
    {
        var memberInfo = NormalizeMemberInfo(member);
        var memberDocumentation = _memberData.GetValueOrDefault(memberInfo);

        return memberDocumentation;
    }

    private MemberInfo NormalizeMemberInfo(MemberInfo member)
    {
        // validate the MemberInfo, it must be declared in this type
        if (member.DeclaringType != Type)
        {
            throw new InvalidOperationException("The provided property is not declared in this type.");
        }

        // If a reflected MemberInfo is provided, resolve it to the actual member
        if (member.DeclaringType != member.ReflectedType)
        {
            return member switch
            {
                PropertyInfo propertyInfo => member.DeclaringType.GetProperty(propertyInfo.Name)!,
                MethodInfo methodInfo => member.DeclaringType.GetMethod(methodInfo.Name)!,
                FieldInfo fieldInfo => member.DeclaringType.GetField(fieldInfo.Name)!,
                _ => throw new NotSupportedException($"Members of type '{member.GetType().Name}' are not supported.")
            };
        }

        return member;
    }

    /// <summary>
    /// Adds a <see cref="MemberDocumentation{TMemberInfo}"/>
    /// for a member of this <see cref="System.Type"/>
    /// to <see cref="MemberData"/>.
    /// </summary>
    /// <typeparam name="T">Type of the declared member.</typeparam>
    /// <param name="memberInfo">Declared member of this <see cref="System.Type"/>.</param>
    /// <param name="documentation">The <see cref="MemberDocumentation{TMemberInfo}"/> to add.</param>
    internal void AddMemberData<T>(MemberInfo memberInfo, MemberDocumentation<T> documentation)
        where T : MemberInfo
    {
        _memberData.Add(memberInfo, documentation);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Documentation for Type '{Type.Name!}'";
}