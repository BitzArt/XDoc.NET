using System.Xml;

namespace BitzArt.XDoc.Tests;

public class TestClass
{
}

public class PlainTextRendererTests
{
    [Fact]
    public void Render_ExtractsPropertySummary_FromDocumentation()
    {
        // Arrange
        var assembly = GetType().Assembly;

        List<TestMemberNode> nodes =
        [
            new TestMemberNode(typeof(TestClass), "none"),

            new TestMemberNode(TestNodeType.Property,
                "BitzArt.XDoc.Tests.BaseTestClass.Name",
                "<member name=\"P:BitzArt.XDoc.Tests.TestClass.TestProperty\"><summary>Test property documentation.</summary></member>")
        ];

        var xml = nodes.GetXml(assembly);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        var node = xmlDocument.SelectSingleNode("//member[@name='P:BitzArt.XDoc.Tests.TestClass.TestProperty']");

        var memberDocumentation = new FakeMemberDocumentation(node);

        var plainTextRenderer = new PlainTextRenderer(new XDoc());
        var comment = plainTextRenderer.Render(memberDocumentation);

        Assert.Equal("Test property documentation.", comment);
    }

    [Fact]
    public void Render_ReturnsEmptyString_WhenInheritDocTagProvided()
    {
        // Arrange
        var assembly = GetType().Assembly;


        List<TestMemberNode> nodes =
        [
            new TestMemberNode(typeof(TestClass), "node with inherit doc test"),
            new TestMemberNode(TestNodeType.Property,
                "BitzArt.XDoc.Tests.TestClass.Name",
                "<member name=\"P:BitzArt.XDoc.Tests.TestClass.Name\"><inheritdoc /></member>")
        ];

        var xml = nodes.GetXml(assembly);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        var node = xmlDocument.SelectSingleNode("//inheritdoc");

        var memberDocumentation = new FakeMemberDocumentation(node);

        var plainTextRenderer = new PlainTextRenderer(new XDoc());
        var comment = plainTextRenderer.Render(memberDocumentation);

        Assert.Equal("", comment);
    }

    [Fact]
    public void Render_ReturnsEmptyString_WhenDocumentationIsNull()
    {
        // Act
        var result = new PlainTextRenderer(new XDoc()).Render(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Render_ReturnsPlainText_WhenTextNodeProvided()
    {
        // Arrange
        var textNode = new XmlDocument().CreateTextNode("Hello World");
        var memberDocumentation = new FakeMemberDocumentation(textNode);
        var plainTextRenderer = new PlainTextRenderer(new XDoc());

        // Act
        var result = plainTextRenderer.Render(memberDocumentation);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Render_NormalizesMultipleLinesToSingleLine_WhenForceSingleLineIsTrue()
    {
        // Arrange
        var element = new XmlDocument().CreateElement("para");
        element.InnerXml = "Line1\n   Line2\nLine3";

        var memberDocumentation = new FakeMemberDocumentation(element);
        var plainTextRenderer = new PlainTextRenderer(new XDoc());

        // Act
        var result = plainTextRenderer.Render(memberDocumentation);

        // Assert
        Assert.Equal("Line1 Line2 Line3", result);
    }

    [Fact]
    public void Render_RendersCrefReference_ReturnsShortTypeName()
    {
        // Arrange
        var xmlDocument = new XmlDocument();
        var xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "test", null);
        var element = xmlDocument.CreateElement("see");

        element.SetAttribute("cref", "T:BitzArt.XDoc.Models.SomeType");
        xmlNode.AppendChild(element);

        xmlDocument.AppendChild(xmlNode);

        var memberDocumentation = new FakeMemberDocumentation(xmlNode);
        var plainTextRenderer = new PlainTextRenderer(new XDoc());

        // Act
        var result = plainTextRenderer.Render(memberDocumentation);

        // Assert
        Assert.Equal("SomeType", result);
    }

    [Fact]
    public void Render_RendersCrefReference_ReturnsMethodName()
    {
        // Arrange
        var xmlDocument = new XmlDocument();
        var xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "test", null);
        var element = xmlDocument.CreateElement("see");

        element.SetAttribute("cref", "M:BitzArt.XDoc.Models.SomeType.SomeMethod");
        xmlNode.AppendChild(element);

        xmlDocument.AppendChild(xmlNode);

        var memberDocumentation = new FakeMemberDocumentation(xmlNode);
        var plainTextRenderer = new PlainTextRenderer(new XDoc());

        // Act
        var result = plainTextRenderer.Render(memberDocumentation);

        // Assert
        Assert.Equal("SomeType.SomeMethod", result);
    }

    [Fact]
    public void Render_ReturnsEmptyString_WhenXmlElementIsEmpty()
    {
        // Arrange
        var element = new XmlDocument().CreateElement("empty");
        var memberDocumentation = new FakeMemberDocumentation(element);
        var plainTextRenderer = new PlainTextRenderer(new XDoc());

        // Act
        var result = plainTextRenderer.Render(memberDocumentation);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}