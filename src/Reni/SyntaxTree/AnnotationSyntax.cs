using Reni.Basics;
using Reni.Context;
using Reni.SyntaxFactory;
using Reni.TokenClasses;

namespace Reni.SyntaxTree;

sealed class AnnotationSyntax : ValueSyntax
{
    readonly ValueSyntax Target;
    readonly IValueAnnotation Annotation;

    AnnotationSyntax(ValueSyntax target, IValueAnnotation annotation, Anchor anchor)
        : base(anchor)
    {
        Target = target;
        Annotation = annotation;
    }

    protected override int DirectChildCount => 1;

    protected override Syntax GetDirectChild(int index) => index == 0? Target : null;

    internal override Result GetResultForCache(ContextBase context, Category category)
    {
        var target = context.GetResultAsReference(category | Category.Type, Target);
        return Annotation.GetFeature(target.Type).Value.Execute(category)
            .ReplaceArguments(target);
    }

    internal static AnnotationSyntax Create
        (ValueSyntax target, IValueAnnotation annotation, BinaryTree[] anchors)
        => new(target, annotation, Anchor.Create(anchors));
}