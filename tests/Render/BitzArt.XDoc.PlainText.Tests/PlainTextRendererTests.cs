using System.Xml;
using TestAssembly.B;

namespace BitzArt.XDoc.Tests;

public class PlainTextRendererTests
{
    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnInheritedProperty()
    {
        // Arrange
        var xDoc = new XDoc();
        var propertyDocumentation = xDoc.Get(typeof(Dog).GetProperty("Color"));
        
        // Act
        var comment = propertyDocumentation.ToPlainText();
       
        // Assert
        Assert.NotNull(comment);
    }
    
    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnCrefProperty()
    {
        // Arrange
        var xDoc = new XDoc();
        var propertyDocumentation = xDoc.Get(typeof(Dog).GetProperty("Field2"));
        
        // Act
        var comment = propertyDocumentation.ToPlainText();
       
        // Assert
        Assert.NotNull(comment);
    }
    
    [Fact]
    public void Render_PlainTextRenderer_ShouldReturnFormattedText()
    {
        // Arrange
        var xml = @"<member name=""P:MyCompany.Library.WeeklyMetrics.SuccessRatio"">
                        <summary>
                            The ratio of <see cref=""P:MyCompany.Library.WeeklyMetrics.Progress""/> to <see cref=""P:MyCompany.Library.WeeklyMetrics.Objective""/> for this <see cref=""T:MyCompany.Library.WeeklyMetric""/>.
                        </summary>
                    </member>";

        var xmlNode = GetXmlNode(xml);


        // Act
        var memberDocumentation = new TestPropertyDocumentation(xmlNode);

        var str = PlainTextRenderer.Render(memberDocumentation);

        // Assert
        Assert.Contains("The ratio of Progress to Objective for this WeeklyMetric.", str);
    }


    [Fact]
    public void RenderInheritedDocumentation_PlainTextRenderer_ShouldReturnFormattedText()
    {
        // Act
        var source = new XDoc();
        var propertyDocumentation = source.Get(typeof(Dog).GetProperty(nameof(Dog.Property1)));
        var text = propertyDocumentation.ToPlainText();


        // Assert
        Assert.Contains("Description of Property1", text);
    }

    private static XmlNode GetXmlNode(string xml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>" + xml);

        XmlNode xmlNode = xmlDoc.DocumentElement!; // Get the node

        return xmlNode;
    }
}

public class TestPropertyDocumentation : MemberDocumentation
{
    public TestPropertyDocumentation(XmlNode xmlNode)
        : base(new XDoc(), xmlNode)
    {
    }
}