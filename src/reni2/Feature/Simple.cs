using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    abstract class ConversionBase : DumpableObject, IConversion
    {
        static int _nextObjectId;

        protected ConversionBase(Func<Category, Result> function, TypeBase source)
            : base(_nextObjectId++)
        {
            Function = function;
            Source = source;
            Tracer.Assert(Source != null);
        }

        [EnableDump]
        internal Func<Category, Result> Function { get; }
        TypeBase Source { get; }

        Result IConversion.Result(Category category) => Function(category);
        TypeBase IConversion.Source => Source;

        protected override string GetNodeDump()
        {
            return Source.DumpPrintText
                + "-->"
                + (Function(Category.Type).Type?.DumpPrintText ?? "<unknown>" )
                + " MethodName="
                + Function.Method.Name;
        }
    }

    sealed class Conversion : ConversionBase, IImplementation
    {
        public Conversion(Func<Category, Result> function, TypeBase source)
            : base(function, source) { }

        IMeta IMetaImplementation.Function => null;
        IFunction IEvalImplementation.Function => null;
        IConversion IEvalImplementation.Conversion => this;
    }

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
        Result IConversion.Result(Category category) => Right.Result(category).ReplaceArg(Left.Result);
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