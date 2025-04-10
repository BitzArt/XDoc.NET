using System.Reflection;

namespace BitzArt.XDoc;

/// <summary>
/// Contains documentation for a specific <see cref="MemberInfo"/>
/// </summary>
public interface IMemberDocumentation
{
    /// <summary>
    /// The <see cref="MemberInfo"/> this documentation is provided for.
    /// </summary>
    public MemberInfo Member { get; }
}
