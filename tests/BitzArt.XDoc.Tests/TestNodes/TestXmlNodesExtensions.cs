using System.Reflection;

namespace BitzArt.XDoc.Tests;

public static class TestXmlNodesExtensions
{
    public static string GetXml(this IEnumerable<TestMemberNode> nodes, Assembly assembly) =>
        $"""
        <?xml version="1.0"?>
        <doc>
            <assembly>
                <name>{assembly.GetName().Name}</name>
            </assembly>
            <members>
                {string.Join('\n', nodes.Select(node => node.GetXml().Offset(8, exceptFirstLine: true)))}
            </members>
        </doc>
        """;
}
