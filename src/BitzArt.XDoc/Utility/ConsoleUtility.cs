using System.Diagnostics;

namespace BitzArt.XDoc;

/// <summary>
/// Provides utility methods for writing colored messages to the console and logging them via <see cref="Trace"/>.
/// </summary>
internal class ConsoleUtility
{
    /// <summary>
    /// Writes a message to the console with the specified foreground color and logs it using <see cref="Trace"/>.
    /// </summary>
    /// <param name="message">The message to write to the console and log.</param>
    /// <param name="foregroundColor">The color to use for the console text. Defaults to <see cref="ConsoleColor.Black"/>.</param>
    internal static void WriteLine(string message, ConsoleColor foregroundColor = ConsoleColor.Black)
    {
        Console.ForegroundColor = foregroundColor;
        Console.WriteLine(message);
        Console.ResetColor();
        
        Trace.WriteLine(message);
    }
}