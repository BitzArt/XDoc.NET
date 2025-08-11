using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace XDoc;

/// <summary>
/// Extension methods for PropertyBuilder.
/// </summary>
[PublicAPI]
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Adds a comment to a property being configured, using documentation from XDoc.
    /// The comment is extracted from a property defined in the provided expression.
    /// </summary>
    /// <typeparam name="TCommentTargetEntity">The entity type containing the property whose documentation will be used</typeparam>
    /// <typeparam name="TCommentTargetProperty">The type of the property whose documentation will be used</typeparam>
    /// <param name="propertyBuilder">The builder for the property</param>
    /// <param name="xdoc">The XDoc instance used to extract documentation</param>
    /// <param name="propertyName">The name of the property to configure</param>
    /// <param name="commentTargetPropertyExpression">An expression that identifies the property whose documentation will be used</param>
    /// <returns>The same property builder instance so that multiple calls can be chained</returns>
    public static PropertyBuilder HasComment<TCommentTargetEntity, TCommentTargetProperty>(
        this PropertyBuilder propertyBuilder,
        XDoc xdoc,
        string propertyName,
        Expression<Func<TCommentTargetEntity, TCommentTargetProperty>> commentTargetPropertyExpression)
        where TCommentTargetEntity : class
    {
        var documentationElement = xdoc.Get(commentTargetPropertyExpression);

        if (documentationElement is null)
        {
            return propertyBuilder;
        }

        var comment = documentationElement.ToPlainText();

        return propertyBuilder.HasComment(comment);
    }

    /// <summary>
    /// Adds a comment to a property being configured, using documentation from XDoc.
    /// The comment is extracted from a property with the specified name in the target entity type.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being configured</typeparam>
    /// <typeparam name="TCommentTargetEntity">The entity type containing the property whose documentation will be used</typeparam>
    /// <typeparam name="TCommentTargetProperty">The type of the property in the expression parameter (not directly used)</typeparam>
    /// <param name="propertyBuilder">The builder for the property</param>
    /// <param name="xdoc">The XDoc instance used to extract documentation</param>
    /// <param name="commentTargetPropertyExpression">An expression that identifies a property (note: this parameter is not used in the method body)</param>
    /// <returns>The same property builder instance so that multiple calls can be chained</returns>
    public static PropertyBuilder<TProperty> HasComment<TProperty, TCommentTargetEntity, TCommentTargetProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        XDoc xdoc,
        Expression<Func<TCommentTargetEntity, TCommentTargetProperty>> commentTargetPropertyExpression)
        where TCommentTargetEntity : class
    {
        var documentationElement = xdoc.Get(commentTargetPropertyExpression);

        if (documentationElement is null)
        {
            return propertyBuilder;
        }

        var comment = documentationElement.ToPlainText();

        return propertyBuilder.HasComment(comment);
    }
}