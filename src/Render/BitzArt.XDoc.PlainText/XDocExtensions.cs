namespace BitzArt.XDoc;

/// <summary>
/// Extension methods for <see cref="XDoc"/> objects which allow rendering documentation in plain text.
/// </summary>
public static class XDocExtensions
{
    /// <summary>
    /// Renders the documentation of a <see cref="Type"/> as plain text.
    /// </summary>
    /// <param name="documentation"></param>
    /// <param name="useShortTypeNames"></param>
    /// <param name="forceSingleLine"></param>
    /// <returns></returns>
    public static string ToPlainText(
        this MemberDocumentation? documentation,
        bool forceSingleLine = false,
        bool useShortTypeNames = true)
    {
        if (documentation == null)
        {
            return string.Empty;
        }

        return PlainTextRenderer.Render(documentation, forceSingleLine, useShortTypeNames);
    }
}