using System.Reflection;
using System.Xml;

namespace BitzArt.XDoc.Tests;

public class XmlParserTests
{
    [Fact]
    public void Parse_EmptyDoc_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var assembly = GetType().Assembly;

        var nodes = new List<TestMemberNode>();

        var xml = nodes.GetXml(assembly);

        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var parser = new XmlParser(new XDoc(), assembly, doc);

        // Act
        var results = parser.Parse();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void Parse_XmlDocWithTypeNode_ShouldReturnTypeDocumentation()
    {
        // Arrange
        var assembly = GetType().Assembly;

        var content = "blah";

        var testType = typeof(TestClass);
        var testNode = new TestMemberNode(testType, content);

        var xml = new TestMemberNode[] { testNode }.GetXml(assembly);

        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var parser = new XmlParser(new XDoc(), assembly, doc);

        // Act
        var results = parser.Parse();

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);

        var entry = results.First();
        var type = entry.Key;
        var documentation = entry.Value;

        Assert.Equal(typeof(TestClass), type);

        Assert.NotNull(documentation);
        Assert.Empty(documentation.MemberData);

        var fetchedXmlData = documentation.Node!.InnerXml;

        // `Trim` since the XML data is formatted with newlines and spaces
        var trimmed = fetchedXmlData.Trim();
        Assert.Equal(content, trimmed);
    }

    [Theory]
    [InlineData(TestMemberType.Property)]
    [InlineData(TestMemberType.Field)]
    [InlineData(TestMemberType.Method)]
    public void Parse_XmlDocWithMemberNode_ShouldReturnMemberDocumentationInsideItsTypeDocumentation(TestMemberType memberType)
    {
        // Arrange
        var assembly = GetType().Assembly;

        var content = "blah";

        var testType = typeof(TestClass);

        MemberInfo testMember = memberType switch
        {
            TestMemberType.Property => testType.GetProperty(nameof(TestClass.TestProperty))!,
            TestMemberType.Field => testType.GetField(nameof(TestClass.TestField))!,
            TestMemberType.Method => testType.GetMethod(nameof(TestClass.TestMethod))!,
            _ => throw new ArgumentOutOfRangeException(nameof(memberType))
        };

        var testNode = testMember switch
        {
            PropertyInfo property => new TestMemberNode(property, content),
            FieldInfo field => new TestMemberNode(field, content),
            MethodInfo method => new TestMemberNode(method, content),
            _ => throw new InvalidOperationException("Invalid member type.")
        };

        var xml = new TestMemberNode[] { testNode }.GetXml(assembly);

        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var parser = new XmlParser(new XDoc(), assembly, doc);

        // Act
        var results = parser.Parse();

        // Assert
        Assert.NotNull(results);

        // `Single` because only 1 type is used - it's member data is inside of it
        Assert.Single(results);

        var typeDoc = results.First().Value;

        Assert.NotNull(typeDoc.MemberData);
        Assert.Single(typeDoc.MemberData);

        var member = typeDoc.MemberData.First();
        
        Assert.Equal(testMember, member.Key);

        Assert.NotNull(member.Value);
        switch (memberType)
        {
            case TestMemberType.Property:
                Assert.IsType<PropertyDocumentation>(member.Value);
                break;
            case TestMemberType.Field:
                Assert.IsType<FieldDocumentation>(member.Value);
                break;
            case TestMemberType.Method:
                Assert.IsType<MethodDocumentation>(member.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(memberType));
        }
        var memberDocumentation = member.Value;

        var fetchedXmlData = memberDocumentation.Node!.InnerXml;

        // `Trim` since the XML data is formatted with newlines and spaces
        var trimmed = fetchedXmlData.Trim();
        Assert.Equal(content, trimmed);
    }

    public enum TestMemberType
    {
        Property,
        Field,
        Method
    }
}
