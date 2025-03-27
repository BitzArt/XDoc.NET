namespace BitzArt.XDoc.Tests;

public class AssemblyDocumentationTests
{
    [Fact]
    public void GetDocumentation_TypeFromAnotherAssembly_ShouldThrow()
    {
        // Arrange
        var assembly = GetType().Assembly;
        var assemblyDocumentation = new AssemblyDocumentation(new XDoc(), assembly, []);

        var testType = typeof(object);

        // Act + Assert
        Assert.ThrowsAny<Exception>(() => assemblyDocumentation.GetDocumentation(testType));
    }

    [Fact]
    public void GetDocumentation_TypeWithoutDocumentation_ShouldReturnNull()
    {
        // Arrange
        var assembly = GetType().Assembly;
        var assemblyDocumentation = new AssemblyDocumentation(new XDoc(), assembly, []);
        var testType = typeof(TestClass);
        
        // Act
        var result = assemblyDocumentation.GetDocumentation(testType);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDocumentation_TypeWithDocumentation_ShouldReturnDocumentation()
    {
        // Arrange
        var assembly = GetType().Assembly;

        var testType = typeof(TestClass);

        var typeDocumentation = new TypeDocumentation(null!, testType, null);
        var typeDict = new Dictionary<Type, TypeDocumentation>
        {
            { testType, typeDocumentation }
        };
        var assemblyDocumentation = new AssemblyDocumentation(null!, assembly, typeDict);

        // Act
        var result = assemblyDocumentation.GetDocumentation(testType);

        // Assert
        Assert.NotNull(result);
        Assert.Same(typeDocumentation, result);
    }
}
