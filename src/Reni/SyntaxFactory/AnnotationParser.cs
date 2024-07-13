using hw.DebugFormatter;
using hw.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

static class AnnotationParser
{
    internal static(BinaryTree item, (BinaryTree annotation, BinaryTree[] anchors)[] annotations) 
        CheckForAnnotations(this BinaryTree target)
    {
        if(target == null)
            return (item: null, annotations: null);
        if(target.TokenClass is not ExclamationBoxToken)
            return (item: target, annotations: null);

        var (item, leftAnnotations) = target.Left.CheckForAnnotations();
        var rightAnnotations = ParseAnnotations(target.Right);
        AddAnchors(rightAnnotations, T(target));
        var annotations = T(leftAnnotations, rightAnnotations).ConcatMany().ToArray();

        return (item, annotations);
    }

    static(BinaryTree annotation, BinaryTree[] anchors)[] ParseAnnotations(BinaryTree target)
    {
        if(target.BracketKernel is { } bracketKernel)
        {
            var annotations = ParseAnnotations(bracketKernel.Center);
            AddAnchors(annotations, T(bracketKernel.Left, bracketKernel.Right));
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

        return T((target,(BinaryTree[])null));
    }

    static void AddAnchors
        ((BinaryTree annotation, BinaryTree[] anchors)[] target, params BinaryTree[][] anchorsList)
    {
        if(anchorsList.Length == 0)
            return;

        var anchors = anchorsList.ConcatMany().ToArray();

        if(target.Length == 0)
        {
            Dumpable.NotImplementedFunction(target, anchorsList);
            return;
        }

        target[0].anchors = T(target[0].anchors, anchors).ConcatMany().ToArray();
    }

    public static TValue[] T<TValue>(params TValue[] value) => value;
}