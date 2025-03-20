using TestAssembly.A;
using TestAssembly.B;

namespace BitzArt.XDoc.Tests;

public class PlainTextRendererTests
{
    // Act
    private readonly XDoc _xDoc = new XDoc(new SimpleDocumentationReferenceResolver());

    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnInheritedProperty()
    {
        // Arrange
        var propertyDocumentation = _xDoc.Get(typeof(Dog).GetProperty(nameof(Dog.Color))!);

        // Act
        var comment = propertyDocumentation.ToPlainText();

        // Assert
        Assert.NotNull(comment);
    }

    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnCrefProperty()
    {
        // Arrange
        var propertyDocumentation = _xDoc.Get(typeof(Dog).GetProperty(nameof(Dog.Field2))!);

        // Act
        var comment = propertyDocumentation.ToPlainText();

        // Assert
        Assert.NotNull(comment);

        // Assert.Equal("123 Name of specific Dog.\nBe carefully with this property.", comment);
    }

    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnOtherClassCrefProperty()
    {
        // Arrange
        var propertyDocumentation = _xDoc.Get(typeof(Dog).GetProperty(nameof(Dog.Field3))!);

        // Act
        var comment = propertyDocumentation.ToPlainText();

        // Assert
        Assert.Equal("Field same as Cat.Weight", comment);
    }

    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnTypeComment()
    {
        // Arrange
        var propertyDocumentation = _xDoc.Get(typeof(Dog));

        // Act
        var comment = propertyDocumentation.ToPlainText();

        // Assert
        Assert.Equal("This is a class which represents a dog as defined by the interface IDog.", comment);
    }

    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnBaseTypeComment()
    {
        // Arrange
        var propertyDocumentation = _xDoc.Get(typeof(Cat));

        // Act
        var comment = propertyDocumentation.ToPlainText();

        // Assert
        Assert.NotNull(comment);

        // Assert.Equal("Animal class", comment);
    }

    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnFormattedText()
    {
        // Arrange
        const string xml = @"<member name=""P:MyCompany.Library.WeeklyMetrics.SuccessRatio"">
                        <summary>
                            The ratio of <see cref=""P:MyCompany.Library.WeeklyMetrics.Progress""/> to <see cref=""P:MyCompany.Library.WeeklyMetrics.Objective""/> for this <see cref=""T:MyCompany.Library.WeeklyMetric""/>.
                        </summary>
                    </member>";

        var memberDocumentation = new TestPropertyDocumentation(xml);

        // Act
        var comment = memberDocumentation.ToPlainText();

        // Assert
        Assert.Contains("The ratio of WeeklyMetrics.Progress to WeeklyMetrics.Objective for this WeeklyMetric.", comment);
    }

    [Fact]
    public void RenderInheritedDocumentation_PlainTextRenderer_ShouldReturnFormattedText()
    {
        // Arrange
        var propertyDocumentation = _xDoc.Get(typeof(Dog).GetProperty(nameof(Dog.Property1))!);

        // Act
        var text = propertyDocumentation.ToPlainText();

        // Assert
        Assert.NotNull(text);

        // Assert.Contains("Description of Property1", text);
    }
}