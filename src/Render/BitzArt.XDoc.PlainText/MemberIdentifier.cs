namespace BitzArt.XDoc.PlainText;

/// <summary>
/// Represents an identifier for types and members in XML documentation comments.
/// </summary>
internal record MemberIdentifier
{
    private MemberIdentifier(string prefix, string type, string shortType, string? member)
    {
        Prefix = prefix;
        Type = type;
        ShortType = shortType;
        Member = member;
    }

    /// <summary>
    /// Determines if the identifier is a type reference (e.g. "T:").
    /// </summary>
    public bool IsType => Prefix is "T:";

    /// <summary>
    /// Determines if the identifier is a member reference (e.g. "M:", "P:", "F:").
    /// </summary>
    public bool IsMember => Prefix is "M:" or "P:" or "F:";

    /// <summary>
    /// The prefix of the identifier.
    /// Can be: "T:", "M:", "P:", "F:".
    /// </summary>
    public string Prefix { get; private init; }

    /// <summary>
    /// The type name
    /// </summary>
    public string Type { get; private init; }

    /// <summary>
    /// The short type name (without namespace)
    /// </summary>
    public string ShortType { get; private init; }

    /// <summary>
    /// The member name (e.g. method, property, field).
    /// </summary>
    public string? Member { get; private init; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// Formats the identifier back into a valid member or type identifier string.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{Prefix}{Type}{(Member is not null ? "." + Member : string.Empty)}";

    /// <summary>
    /// List if supported member prefixes.
    /// </summary>
    private static readonly IReadOnlyCollection<string> AllowedPrefixes = ["T:", "M:", "P:", "F:"];

    /// <summary>
    /// Attempts to create a <see cref="MemberIdentifier"/> from a string value.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the created <see cref="MemberIdentifier"/> if successful; otherwise, null.
    /// </param>
    /// <returns>true if the creation was successful; otherwise, false.</returns>
    public static bool TryCreate(string value, out MemberIdentifier? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Length < 3 || value[1] != ':')
        {
            return false;
        }

        var prefix = value[..2];

        if (!AllowedPrefixes.Contains(prefix))
        {
            return false;
        }

        var lastIndexOfDot = value.LastIndexOf('.');

        var type = prefix switch
        {
            "T:" => value.Substring(2, value.Length - 2),
            "M:" or "P:" or "F:" => value.Substring(2, lastIndexOfDot - 2),
            _ => string.Empty
        };

        var member = prefix switch
        {
            "T:" => null,
            "M:" or "P:" or "F:" => value.Substring(lastIndexOfDot + 1, value.Length - lastIndexOfDot - 1),
            _ => string.Empty
        };

        var typeLastIndexOfDot = type.LastIndexOf('.');
        var shortType = type.Substring(typeLastIndexOfDot + 1, type.Length - typeLastIndexOfDot - 1);

        result = new MemberIdentifier(prefix, type, shortType, member);

        return true;
    }
}