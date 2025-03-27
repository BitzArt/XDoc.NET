using System.Xml;

namespace BitzArt.XDoc.Tests;

public class TypeDocumentationTests
{
    [Fact]
    public void GetDocumentation_ForProperty_ShouldReturnPropertyDocumentation()
    {
        // Arrange
        var source = new XDoc();

        var xmlDocument = new XmlDocument();
        var node = xmlDocument.CreateTextNode("blah");

        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty))!;
        var propertyDocumentation = new PropertyDocumentation(source, propertyInfo, node);

        var typeDocumentation = new TypeDocumentation(source, typeof(TestClass), node);
        typeDocumentation.AddMemberData(propertyInfo, propertyDocumentation);

        // Act
        var result = typeDocumentation.GetDocumentation(propertyInfo);

        // Assert
        Assert.Same(propertyDocumentation, result);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetDocumentation_IndirectPropertyInfo_ShouldReturnPropertyDocumentation()
    {
        // 'Indirect' MemberInfo, meaning a MemberInfo
        // where DeclaringType is not the same as ReflectedType
        // (e.g. Property inherited from a base class,
        // or a property that is declared in an interface that the class implements)

        // Arrange
        var source = new XDoc();

        var xmlDocument = new XmlDocument();
        var node = xmlDocument.CreateTextNode("blah");
        var inheritedPropertyInfo = typeof(InheritedTestClass).GetProperty(nameof(InheritedTestClass.TestProperty))!;
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty))!;

        var propertyDocumentation = new PropertyDocumentation(source, propertyInfo, node);
        var inheritedPropertyDocumentation = new PropertyDocumentation(source, inheritedPropertyInfo, node);

        var typeDocumentation = new TypeDocumentation(source, typeof(TestClass), node);
        typeDocumentation.AddMemberData(inheritedPropertyInfo, inheritedPropertyDocumentation);
        typeDocumentation.AddMemberData(propertyInfo, propertyDocumentation);

        // Act
        var resolvedPropertyDocumentation = typeDocumentation.GetDocumentation(inheritedPropertyInfo);

        // Assert
        Assert.Same(propertyDocumentation, resolvedPropertyDocumentation);
        Assert.NotNull(resolvedPropertyDocumentation);
    }

    [Fact]
    public void GetDocumentation_PropertyNotDeclaredInThisType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var source = new XDoc();

        var xmlDocument = new XmlDocument();
        var node = xmlDocument.CreateTextNode("blah");
        var inheritedPropertyInfo = typeof(InheritedTestClass).GetProperty(nameof(InheritedTestClass.TestProperty))!;

        var inheritedPropertyDocumentation = new PropertyDocumentation(source, inheritedPropertyInfo, node);

        var typeDocumentation = new TypeDocumentation(source, typeof(InheritedTestClass), node);
        typeDocumentation.AddMemberData(inheritedPropertyInfo, inheritedPropertyDocumentation);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            // This should throw an InvalidOperationException,
            // since the property is not declared in this type
            // (but rather in it's base class)
            var resolvedPropertyDocumentation = typeDocumentation.GetDocumentation(inheritedPropertyInfo);
        });
    }
}