using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

sealed class Annotation(BinaryTree? value = null, BinaryTree[]? anchors = null)
{
    public readonly BinaryTree? Value = value;
    public BinaryTree[] Anchors = anchors??[];
}

static class AnnotationParser
{
    internal static(BinaryTree? item, Annotation[] annotations) CheckForAnnotations(this BinaryTree? target)
    {
        if(target == null)
            return (item: null, annotations: []);
        if(target.TokenClass is not ExclamationBoxToken)
            return (item: target, annotations: []);

        var (item, leftAnnotations) = target.Left.CheckForAnnotations();
        var rightAnnotations = ParseAnnotations(target.Right);
        AddAnchors(rightAnnotations, T(target));
        var annotations = T(leftAnnotations, rightAnnotations).ConcatMany().ToArray();

        return (item, annotations);
    }

    static Annotation[] ParseAnnotations(BinaryTree? target)
    {
        if(target == null)
            return T(new Annotation());

        if(target.BracketKernel is { } bracketKernel)
        {
            var annotations = ParseAnnotations(bracketKernel.Center);
            AddAnchors(annotations, T(bracketKernel.Left!, bracketKernel.Right!));
            return annotations;
        }

        if(target.TokenClass is List)
        {
            var leftAnnotations = ParseAnnotations(target.Left);
            var rightAnnotations = ParseAnnotations(target.Right);
            var annotations = T(leftAnnotations, rightAnnotations).ConcatMany().ToArray();
            AddAnchors(annotations, T(target));
            return annotations;
        }

        target.Right.AssertIsNull();
        if(target.Left == null)
            return T(new Annotation(target));

        return T(new Annotation(anchors:T(target.Left, target)));
    }

    static void AddAnchors
        (Annotation[] target, params BinaryTree[][] anchorsList)
    {
        if(anchorsList.Length == 0)
            return;

        var anchors = anchorsList.ConcatMany().ToArray();

        if(target.Length == 0)
        {
            Dumpable.NotImplementedFunction(target, anchorsList);
            return;
        }

        target[0].Anchors = T(target[0].Anchors, anchors).ConcatMany().ToArray();
    }

    public static TValue[] T<TValue>(params TValue[] value) => value;
}