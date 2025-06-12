using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace BitzArt.XDoc;

/// <summary>
/// Provides utility methods for parsing and resolving information from fully qualified member signatures,
/// such as extracting type names, member names, and method parameter types. 
/// Handles special cases including generic types and nested generic parameters.
/// </summary>
internal static class XmlMemberNameResolver
{
    /// <summary>
    /// Resolves a qualified member name into its associated type and member name.
    /// Handles special cases like generic types and methods with parameters.
    /// </summary>
    /// <param name="xmlDocumentationMemberName">
    /// The fully qualified member name with prefix (e.g. "P:Company.Name.Space.TypeName.MemberName")
    /// </param>
    /// <returns>A tuple containing the resolved Type and the simple member name</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the name doesn't contain a type/member separator or when the type cannot be found
    /// </exception>
    public static (string typeName, string memberName) ResolveTypeAndMemberName(string xmlDocumentationMemberName)
    {
        if (IsMethod(xmlDocumentationMemberName))
        {
            // Remove method parameter information from the XML documentation member name by 
            // truncating the string at the opening parenthesis, keeping only the method name part.
            // This ensures we extract just the method name without its parameter signature.
            xmlDocumentationMemberName = xmlDocumentationMemberName[..xmlDocumentationMemberName.IndexOf('(')];
        }

        EnsureContainsTypeSeparator(xmlDocumentationMemberName);

        // Find the position of the last dot in the member name, which separates
        // the type name from the member name (e.g., "Namespace.TypeName.MemberName" -> position of last dot)
        var indexOfLastDot = xmlDocumentationMemberName.LastIndexOf('.');

        var typeName = xmlDocumentationMemberName[..indexOfLastDot];
        var memberName = xmlDocumentationMemberName[(indexOfLastDot + 1)..];

        if (IsGeneric(xmlDocumentationMemberName))
        {
            if (HasMethodParameters(memberName))
            {
                memberName = memberName[..memberName.IndexOf('(')];
            }

            if (HasGenericTypeParameters(memberName))
            {
                // Remove generic type parameters from the member name
                memberName = memberName[..memberName.IndexOf('`')];
            }
        }

        return (typeName, memberName);
    }

    /// <summary>
    /// Ensures the XML documentation member name contains a type separator (dot).
    /// </summary>
    /// <param name="xmlDocumentationMemberName">The XML documentation member name to check.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the XML documentation member name doesn't contain a type separator.
    /// </exception>
    private static void EnsureContainsTypeSeparator(string xmlDocumentationMemberName)
    {
        var indexOfLastDot = xmlDocumentationMemberName.LastIndexOf('.');

        if (indexOfLastDot == -1)
        {
            throw new InvalidOperationException(
                $"XML documentation member name '{xmlDocumentationMemberName}' does not contain a type separator.");
        }
    }

    /// <summary>
    /// Determines whether the specified member name indicates it contains generic type parameters.
    /// </summary>
    /// <param name="memberName">The member name to check for generic type parameter indicators.</param>
    /// <returns>
    /// <c>true</c> if the member name contains a backtick character (`) indicating generic type parameters;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool HasGenericTypeParameters(string memberName)
    {
        return memberName.Contains('`');
    }

    /// <summary>
    /// Determines whether the specified XML documentation member name represents a generic type or method.
    /// </summary>
    /// <param name="xmlDocumentationMemberName">
    /// The fully qualified XML documentation member name to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the member name contains a backtick character (`) indicating generic type parameters;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool IsGeneric(string xmlDocumentationMemberName)
    {
        return xmlDocumentationMemberName.Contains('`');
    }

    /// <summary>
    /// Determines whether the specified XML documentation member name represents a method.
    /// </summary>
    /// <param name="xmlDocumentationMemberName">
    /// The fully qualified XML documentation member name to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the member name contains an opening parenthesis indicating method parameters;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool IsMethod(string xmlDocumentationMemberName)
    {
        return xmlDocumentationMemberName.Contains('(');
    }

    /// <summary>
    /// Determines whether the specified member name contains method parameters.
    /// </summary>
    /// <param name="memberName">The member name to check for method parameter indicators.</param>
    /// <returns>
    /// <c>true</c> if the member name contains an opening parenthesis indicating method parameters;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool HasMethodParameters(string memberName)
    {
        return memberName.Contains('(');
    }

    /// <summary>
    /// Extracts the method parameter type names from a fully qualified member signature.
    /// Handles nested generic parameters and returns a collection of cleaned parameter type names.
    /// </summary>
    /// <param name="xmlDocumentationMemberName">
    /// The fully qualified member signature, including parameter list (e.g., "Namespace.TypeName.MethodName(System.String, System.Collections.Generic.List&lt;System.Int32&gt;)").
    /// </param>
    /// <returns>
    /// A read-only collection of parameter type names as strings. Returns an empty collection if no parameters are found.
    /// </returns>
    public static IReadOnlyCollection<string> ResolveMethodParameters(string xmlDocumentationMemberName)
    {
        var parameterListStartIndex = xmlDocumentationMemberName.IndexOf('(');

        if (parameterListStartIndex == -1)
        {
            // No parameters found, return an empty collection
            return ImmutableArray<string>.Empty;
        }

        var parameterListEndIndex = xmlDocumentationMemberName.LastIndexOf(')');

        if (parameterListEndIndex <= parameterListStartIndex)
        {
            // No valid parameter list found, return an empty collection
            return ImmutableArray<string>.Empty;
        }

        var parametersString = xmlDocumentationMemberName.Substring(
            parameterListStartIndex + 1,
            parameterListEndIndex - parameterListStartIndex - 1);

        if (string.IsNullOrWhiteSpace(parametersString))
        {
            // No parameters found, return an empty collection
            return ImmutableArray<string>.Empty;
        }

        // Handle nested generic parameters by tracking depth
        return ParseParameterList(parametersString);
    }

    /// <summary>
    /// Parses a parameter list string into individual parameter type names.
    /// Handles nested generic arguments by tracking bracket depth.
    /// </summary>
    /// <param name="parametersString">String containing comma-separated parameter type names.</param>
    /// <returns>A collection of parsed and cleaned parameter type names.</returns>
    private static IReadOnlyCollection<string> ParseParameterList(string parametersString)
    {
        var parameters = new List<string>();
        var currentParam = string.Empty;
        var angleBracketDepth = 0;

        foreach (var c in parametersString)
        {
            if (IsParameterSeparator(c, angleBracketDepth))
            {
                parameters.Add(RemoveGenericMarkers(currentParam.Trim()));
                currentParam = string.Empty;

                continue;
            }

            // Track bracket depth to handle nested generics
            angleBracketDepth = TrackBracketDepth(c, angleBracketDepth);

            currentParam += c;
        }

        // Add the last parameter
        if (!string.IsNullOrWhiteSpace(currentParam))
        {
            parameters.Add(RemoveGenericMarkers(currentParam.Trim()));
        }

        return parameters;
    }

    private static int TrackBracketDepth(char c, int currentBracketDepth)
    {
        if (IsOpeningBracket(c))
        {
            currentBracketDepth++;
        }
        else if (IsClosingBracket(c))
        {
            currentBracketDepth--;
        }

        return currentBracketDepth;
    }

    /// <summary>
    /// Determines whether the current character is a comma that separates parameters at the root level
    /// (not within any generic type arguments or nested structures).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <param name="bracketDepth">The current nesting depth of angle brackets or braces.</param>
    /// <returns>
    /// <c>true</c> if the character is a comma and not inside any nested structure;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool IsParameterSeparator(char c, int bracketDepth) => c is ',' && bracketDepth == 0;

    /// <summary>
    /// Determines whether the specified character is an opening bracket used in generic type parameters
    /// or collection initializers.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>
    /// <c>true</c> if the character is an opening angle bracket '<' or curly brace '{';
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool IsOpeningBracket(char c) => c is '<' or '{';

    /// <summary>
    /// Determines whether the specified character is a closing bracket used in generic type parameters
    /// or collection initializers.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>
    /// <c>true</c> if the character is a closing angle bracket '>' or curly brace '}';
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool IsClosingBracket(char c) => c is '>' or '}';

    /// <summary>
    /// Remove generic markers like `0, `1, etc. and any standalone backticks
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string RemoveGenericMarkers(string value) => Regex.Replace(value, @"`\d+", "").Replace("`", "");
}