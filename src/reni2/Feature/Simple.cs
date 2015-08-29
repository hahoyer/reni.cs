using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    abstract class ValueBase : DumpableObject, IValueFeature
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

        Result IValueFeature.Result(Category category) => Function(category);
        TypeBase IValueFeature.TargetType => Target;

        protected override string GetNodeDump()
        {
            return Target.DumpPrintText
                + "-->"
                + (Function(Category.Type).Type?.DumpPrintText ?? "<unknown>" )
                + " MethodName="
                + Function.Method.Name;
        }
    }

    sealed class Value : ValueBase, IFeatureImplementation
    {
        public Value(Func<Category, Result> function, TypeBase type)
            : base(function, type) { }

        IContextMetaFunctionFeature IContextMetaFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IMetaFeatureImplementation.Meta => null;
        IFunctionFeature ITypedFeatureImplementation.Function => null;
        IValueFeature ITypedFeatureImplementation.Value => this;
    }

    sealed class Combination : DumpableObject, IValueFeature, IEquatable<IValueFeature>
    {
        IValueFeature Left { get; }
        IValueFeature Right { get; }

        public static IValueFeature CheckedCreate(IValueFeature left, IValueFeature right)
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

        Combination(IValueFeature left, IValueFeature right)
        {
            Left = left;
            Right = right;
        }
        Result IValueFeature.Result(Category category) => Right.Result(category).ReplaceArg(Left.Result);
        TypeBase IValueFeature.TargetType => Left.TargetType;

        bool IEquatable<IValueFeature>.Equals(IValueFeature other)
        {
            var typedOther = other as Combination;
            if(typedOther == null)
                return false;
            return Left == typedOther.Left
                && Right == typedOther.Right;
        }
    }
}