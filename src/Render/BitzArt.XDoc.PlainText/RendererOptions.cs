namespace BitzArt.XDoc;

/// <summary>
/// Configuration options for controlling XML documentation rendering behavior.
/// </summary>
public record RendererOptions
{
    /// <summary>
    /// Indicating whether the content should be forced to render as a single line.
    /// When set to true, any line breaks in the content will be removed or replaced.
    /// When set to false, the content will maintain its original line structure.
    /// </summary>
    public bool ForceSingleLine { get; init; } = true;

    /// <summary>
    /// Indicating whether type names should be rendered in their short form.
    /// When set to true, simple type names will be used (e.g., "string" instead of "System.String").
    /// When set to false, fully qualified type names will be used.
    /// </summary>
    public bool UseShortTypeNames { get; init; } = true;
}