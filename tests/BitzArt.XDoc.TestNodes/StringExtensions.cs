namespace BitzArt.XDoc.Tests;

public static class StringExtensions
{
    // Offsets the content by the specified number of spaces.
    // In case if the content has multiple lines, each line will be offset.
    public static string Offset(this string content, int offset, bool exceptFirstLine = false)
    {
        var lines = content.Split('\n');

        var offsetString = new string(' ', offset);

        for (var i = exceptFirstLine ? 1 : 0; i < lines.Length; i++)
        {
            lines[i] = $"{offsetString}{lines[i]}";
        }

        return string.Join('\n', lines);
    }
}