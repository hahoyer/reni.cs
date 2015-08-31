using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    abstract class ValueBase : DumpableObject, IValue
    {
        static int _nextObjectId;

        protected ValueBase(Func<Category, Result> function, TypeBase target)
            : base(_nextObjectId++)
        {
            Function = function;
            Target = target;
            Tracer.Assert(Target != null);
        }

        [EnableDump]
        internal Func<Category, Result> Function { get; }
        TypeBase Target { get; }

        Result IValue.Result(Category category) => Function(category);
        TypeBase IValue.TargetType => Target;

        protected override string GetNodeDump()
        {
            return Target.DumpPrintText
                + "-->"
                + (Function(Category.Type).Type?.DumpPrintText ?? "<unknown>" )
                + " MethodName="
                + Function.Method.Name;
        }
    }

    sealed class Value : ValueBase, IImplementation
    {
        public Value(Func<Category, Result> function, TypeBase type)
            : base(function, type) { }

        IMeta IMetaImplementation.Function => null;
        IFunction IEvalImplementation.Function => null;
        IValue IEvalImplementation.Value => this;
    }

    sealed class Combination : DumpableObject, IValue, IEquatable<IValue>
    {
        IValue Left { get; }
        IValue Right { get; }

        public static IValue CheckedCreate(IValue left, IValue right)
        {
            if(left == null)
                return right;
            if(right == null)
                return left;
            Tracer.Assert(left.ResultType() == right.TargetType);
            if(right.ResultType() == left.TargetType)
                return null;

            return new Combination(left, right);
        }

        Combination(IValue left, IValue right)
        {
            Left = left;
            Right = right;
        }
        Result IValue.Result(Category category) => Right.Result(category).ReplaceArg(Left.Result);
        TypeBase IValue.TargetType => Left.TargetType;

        bool IEquatable<IValue>.Equals(IValue other)
        {
            var typedOther = other as Combination;
            if(typedOther == null)
                return false;
            return Left == typedOther.Left
                && Right == typedOther.Right;
        }
    }
}