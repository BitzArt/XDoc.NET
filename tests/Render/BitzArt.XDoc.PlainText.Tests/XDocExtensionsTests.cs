using TestAssembly.A;
using TestAssembly.B;

namespace BitzArt.XDoc.Tests;

public class XDocExtensionsTests
{
    [Fact]
    public void ToPlainText_DogClass_ShouldReturnDescription()
    {
        // Arrange
        var xDoc = new XDoc();

        // Act
        var str = xDoc.Get(typeof(Dog)).ToPlainText();

        // Assert
        Assert.NotEmpty(str);
    }

    [Fact]
    public void ToPlainText_DogNameProperty_ShouldReturnDescription()
    {
        // Arrange
        var xDoc = new XDoc();

        // Act
        var str = xDoc.Get(typeof(Dog).GetProperty(nameof(Dog.Name))!).ToPlainText();

        // Assert
        Assert.NotEmpty(str);
        Assert.Contains("Name of specific Dog.", str);
    }

    [Fact]
    public void ToPlainText_DogProperties_ShouldReturnDescriptions()
    {
        // Arrange
        var xDoc = new XDoc();
        var type = typeof(Dog);
        
        var field2Info = type.GetProperty(nameof(Dog.Field2));
        var nameInfo = type.GetProperty(nameof(Dog.Name));
        var idInfo = type.GetProperty(nameof(Dog.Id));
        
        // Act
        var filed2Comment = xDoc.Get(field2Info!).ToPlainText(forceSingleLine: true);
        var nameComment = xDoc.Get(nameInfo!).ToPlainText(forceSingleLine: true);
        var idComment = xDoc.Get(idInfo!).ToPlainText(forceSingleLine: true);

        // Assert
        Assert.NotEmpty(filed2Comment);
        Assert.NotEmpty(nameComment);
        Assert.NotEmpty(idComment);
    }
}