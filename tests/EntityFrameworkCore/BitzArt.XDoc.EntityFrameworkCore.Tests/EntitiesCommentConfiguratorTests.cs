using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BitzArt.XDoc.Tests;

public class EntitiesCommentConfiguratorTests
{
    private class TestDbContext1(Action<TestDbContext, ModelBuilder> onModelCreating) : TestDbContext(onModelCreating);

    [Fact]
    public void ConfigureComments_SetsCommentsOnNonOwnedEntities()
    {
        // Arrange
        var xDoc = new XDoc();

        var testContext = new TestDbContext1((context, modelBuilder) =>
        {
            modelBuilder.Entity<MyFirstClass>();

            new EntitiesCommentConfigurator(xDoc).ConfigureComments(modelBuilder);
        });

        // Act
        _ = testContext.Model;

        var designTimeModel = testContext.GetService<IDesignTimeModel>();
        var model = designTimeModel.Model;
        var idComment = model.FindEntityType(typeof(MyFirstClass)).FindProperty(nameof(MyFirstClass.Id)).GetComment();

        Assert.Equal(MyFirstClass.IdComment, idComment);
    }
}