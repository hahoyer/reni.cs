using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    sealed class Combination : DumpableObject, IConversion, IEquatable<IConversion>
    {
        IConversion Left { get; }
        IConversion Right { get; }

        public static IConversion CheckedCreate(IConversion left, IConversion right)
        {
            if(left == null)
                return right;
            if(right == null)
                return left;
            Tracer.Assert(left.ResultType() == right.Source);
            if(right.ResultType() == left.Source)
                return null;

            return new Combination(left, right);
        }

        Combination(IConversion left, IConversion right)
        {
            Left = left;
            Right = right;
        }

        Result IConversion.Execute(Category category)
            => Right.Result(category).ReplaceArg(Left.Result);

        TypeBase IConversion.Source => Left.Source;

        bool IEquatable<IConversion>.Equals(IConversion other)
        {
            var typedOther = other as Combination;
            if(typedOther == null)
                return false;
            return Left == typedOther.Left
                && Right == typedOther.Right;
        }
    }
}