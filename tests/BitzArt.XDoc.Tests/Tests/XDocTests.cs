using System.Collections.Concurrent;
using System.Reflection;

namespace BitzArt.XDoc.Tests;

public class XDocTests
{
    [Fact]
    public void Get_PrefetchedAssembly_ReturnsPrefetchedAssemblyDocumentation()
    {
        // Arrange
        var assembly = GetType().Assembly;
        var assemblyDocumentation = new AssemblyDocumentation(null!, assembly, null!);

        var fetchedAssemblies = new ConcurrentDictionary<Assembly, AssemblyDocumentation>
        {
            [assembly] = assemblyDocumentation
        };

        var xdoc = new XDoc(fetchedAssemblies);

        // Act
        var result = xdoc.Get(assembly);

        // Assert
        Assert.NotNull(result);
        Assert.Same(assemblyDocumentation, result);
    }
}
