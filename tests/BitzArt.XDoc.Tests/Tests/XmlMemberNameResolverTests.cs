namespace BitzArt.XDoc.Tests;

public class XmlMemberNameResolverTests
{
    [Fact]
    public void ResolveTypeAndMemberName_GenericMethod_ShouldResolve()
    {
        // Arrange
        var value = "RootNameSpace.ChildNameSpace.MyClass`1.MyMethod(`0)";

        // Act
        var (type, memberName) = XmlMemberNameResolver.ResolveTypeAndMemberName(value);

        // Assert
        Assert.Equal("RootNameSpace.ChildNameSpace.MyClass`1", type);
        Assert.Equal("MyMethod", memberName);
    }
    
    [Fact]
    public void ResolveTypeAndMemberName_GenericMethodWithFewParameters_ShouldResolve()
    {
        // Arrange
        var value = "RootNameSpace.ChildNameSpace.MyExtension.SomeMethod``1(``0,``0)";

        // Act
        var (type, memberName) = XmlMemberNameResolver.ResolveTypeAndMemberName(value);

        // Assert
        Assert.Equal("RootNameSpace.ChildNameSpace.MyExtension", type);
        Assert.Equal("SomeMethod", memberName);
    }
    
    [Fact]
    public void ResolveTypeAndMemberName_GenericMethodWithComplexParameter_ShouldResolve()
    {
        // Arrange
        var value = "RootNameSpace.ChildNameSpace.MyExtension.MyProperty``1(System.Linq.IQueryable{``0},System.Int32)";

        // Act
        var (type, memberName) = XmlMemberNameResolver.ResolveTypeAndMemberName(value);

        // Assert
        Assert.Equal("RootNameSpace.ChildNameSpace.MyExtension", type);
        Assert.Equal("MyProperty", memberName);
    } 
    
    [Fact]
    public void ResolveMethodParameters_GenericMethodWithComplexParameters_ShouldResolve()
    {
        // Arrange
        var value = "RootNameSpace.ChildNameSpace.SomeClass`1.SomeMethod``1(System.Func{System.Linq.IQueryable{`0},System.Linq.IQueryable{``0}},System.Threading.CancellationToken)";
        
        // Act
        var parameters = XmlMemberNameResolver.ResolveMethodParameters(value);

        // Assert
        Assert.Equal(2, parameters.Count);
    }
}