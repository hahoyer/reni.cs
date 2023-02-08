using hw.DebugFormatter;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature;

sealed class Combination : DumpableObject, IConversion, IEquatable<IConversion>
{
    IConversion Left { get; }
    IConversion Right { get; }

    Combination(IConversion left, IConversion right)
    {
        Left = left;
        Right = right;
    }

    Result IConversion.Execute(Category category)
        => Right.GetResult(category).ReplaceArguments(Left.GetResult);

    TypeBase IConversion.Source => Left.Source;

    bool IEquatable<IConversion>.Equals(IConversion other)
    {
        var typedOther = other as Combination;
        if(typedOther == null)
            return false;
        return Left == typedOther.Left
            && Right == typedOther.Right;
    }

    public static IConversion CheckedCreate(IConversion left, IConversion right)
    {
        if(left == null)
            return right;
        if(right == null)
            return left;
        (left.ResultType() == right.Source).Assert();
        if(right.ResultType() == left.Source)
            return null;

        return new Combination(left, right);
    }
}