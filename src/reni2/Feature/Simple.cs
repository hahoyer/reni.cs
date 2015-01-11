using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    abstract class SimpleBase : DumpableObject, ISimpleFeature
    {
        [EnableDump]
        Func<Category, Result> _function;
        readonly TypeBase _target;
        static int _nextObjectId;
        protected SimpleBase(Func<Category, Result> function, TypeBase target)
            : base(_nextObjectId++)
        {
            _function = function;
            _target = target;
            Tracer.Assert(_target != null);
        }
        Result ISimpleFeature.Result(Category category) => _function(category);
        TypeBase ISimpleFeature.TargetType => _target;
        protected override string GetNodeDump()
            => _function(Category.Type).Type.DumpPrintText
                + " <== "
                + _target.DumpPrintText
                + "."
                + _function.Method.Name;
    }

    sealed class Simple : SimpleBase, IFeatureImplementation
    {
        public Simple(Func<Category, Result> function, TypeBase type)
            : base(function, type) { }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => this;
    }

    sealed class Combination : DumpableObject, ISimpleFeature, IEquatable<ISimpleFeature>
    {
        ISimpleFeature Left { get; }
        ISimpleFeature Right { get; }

        static public ISimpleFeature CheckedCreate(ISimpleFeature left, ISimpleFeature right)
        {
            if(left == null)
                return right;
            if(right == null)
                return left;
            Tracer.Assert(left.ResultType() == right.TargetType);
            if(right.ResultType() == left.TargetType)
                return null;

            return new Combination(left,right);
        }

        Combination(ISimpleFeature left, ISimpleFeature right)
        {
            Left = left;
            Right = right;
        }
        Result ISimpleFeature.Result(Category category) => Right.Result(category).ReplaceArg(Left.Result);
        TypeBase ISimpleFeature.TargetType => Left.TargetType;

        bool IEquatable<ISimpleFeature>.Equals(ISimpleFeature other)
        {
            var typedOther = other as Combination;
            if (typedOther == null)
                return false;
            return Left == typedOther.Left
                && Right == typedOther.Right;
        }
    }
}