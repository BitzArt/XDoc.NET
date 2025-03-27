namespace BitzArt.XDoc.Tests;

public class WorkflowTestClass
{
    /// <summary>
    /// Test property documentation.
    /// </summary>
    public string TestPropertyWithDocumentation { get; set; } = null!;

    public string TestPropertyWithoutDocumentation { get; set; } = null!;

    /// <summary>
    /// Test method documentation.
    /// </summary>
    public void TestMethodWithDocumentation() { }

    public void TestMethodWithoutDocumentation() { }

    /// <summary>
    /// Test field documentation.
    /// </summary>
    public string TestFieldWithDocumentation = null!;

    public string TestFieldWithoutDocumentation = null!;
}

public class XDocWorkflowTests
{
    [Fact]
    public void Get_PropertyWithDocumentation_ShouldReturnDocumentation()
    {
        // Arrange
        var xdoc = new XDoc();
        var testType = typeof(WorkflowTestClass);

        // Act
        var propertyDocumentation = xdoc.Get(testType.GetProperty(nameof(WorkflowTestClass.TestPropertyWithDocumentation))!);

        // Assert
        Assert.NotNull(propertyDocumentation);
        Assert.Contains("Test property documentation.", propertyDocumentation!.Node!.InnerXml);
    }

    [Fact]
    public void Get_PropertyWithDocumentation_ShouldReturnNull()
    {
        // Arrange
        var xdoc = new XDoc();
        var testType = typeof(WorkflowTestClass);

        // Act
        var propertyWithoutDocumentation = xdoc.Get(testType.GetProperty(nameof(WorkflowTestClass.TestPropertyWithoutDocumentation))!);

        // Assert
        Assert.Null(propertyWithoutDocumentation);
    }

    [Fact]
    public void Get_MethodWithDocumentation_ShouldReturnDocumentation()
    {
        // Arrange
        var xdoc = new XDoc();
        var testType = typeof(WorkflowTestClass);

        // Act
        var methodDocumentation = xdoc.Get(testType.GetMethod(nameof(WorkflowTestClass.TestMethodWithDocumentation))!);

        // Assert
        Assert.NotNull(methodDocumentation);
        Assert.Contains("Test method documentation.", methodDocumentation!.Node!.InnerXml);
    }

    [Fact]
    public void Get_MethodWithoutDocumentation_ShouldReturnNull()
    {
        // Arrange
        var xdoc = new XDoc();
        var testType = typeof(WorkflowTestClass);

        // Act
        var methodWithoutDocumentation = xdoc.Get(testType.GetMethod(nameof(WorkflowTestClass.TestMethodWithoutDocumentation))!);

        // Assert
        Assert.Null(methodWithoutDocumentation);
    }

    [Fact]
    public void Get_FieldWithDocumentation_ShouldReturnDocumentation()
    {
        // Arrange
        var xdoc = new XDoc();
        var testType = typeof(WorkflowTestClass);

        // Act
        var fieldDocumentation = xdoc.Get(testType.GetField(nameof(WorkflowTestClass.TestFieldWithDocumentation))!);

        // Assert
        Assert.NotNull(fieldDocumentation);
        Assert.Contains("Test field documentation.", fieldDocumentation!.Node!.InnerXml);
    }

    [Fact]
    public void Get_FieldWithoutDocumentation_ShouldReturnNull()
    {
        // Arrange
        var xdoc = new XDoc();
        var testType = typeof(WorkflowTestClass);

        // Act
        var fieldWithoutDocumentation = xdoc.Get(testType.GetField(nameof(WorkflowTestClass.TestFieldWithoutDocumentation))!);

        // Assert
        Assert.Null(fieldWithoutDocumentation);
    }
}