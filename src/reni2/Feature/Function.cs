using System;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Feature
{
    abstract class ObjectFunctionBase : DumpableObject, IFunction
    {
        readonly Func<Category, IContextReference, TypeBase, Result> _function;
        readonly IContextReferenceProvider _target;
        static int _nextObjectId;
        [UsedImplicitly]
        readonly int _order;

        protected ObjectFunctionBase
            (Func<Category, IContextReference, TypeBase, Result> function, IContextReferenceProvider target)
            : base(_nextObjectId++)
        {
            _order = Closures.NextOrder++;
            _function = function;
            _target = target;
        }

        Result IFunction.Result(Category category, TypeBase argsType)
            => _function(category, ObjectReference, argsType);

        bool IFunction.IsImplicit => false;
        IContextReference ObjectReference => _target.ContextReference;
    }

    sealed class ObjectFunction : ObjectFunctionBase, IImplementation
    {
        public ObjectFunction(Func<Category, IContextReference, TypeBase, Result> function, IContextReferenceProvider target)
            : base(function, target) {}

        IMeta IMetaImplementation.Function => null;
        IFunction IEvalImplementation.Function => this;
        IValue IEvalImplementation.Value => null;
    }

    sealed class Function : FunctionFeatureImplementation
    {
        readonly Func<Category, TypeBase, Result> _function;

        internal Function(Func<Category, TypeBase, Result> function) { _function = function; }

        protected override Result Result(Category category, TypeBase argsType) => _function(category, argsType);
        protected override bool IsImplicit => false;
    }

    sealed class ExtendedFunction<T> : FunctionFeatureImplementation
    {
        static int _nextObjectId;
        [UsedImplicitly]
        readonly int _order;

        readonly Func<Category, TypeBase, T, Result> _function;
        readonly T _arg;

        public ExtendedFunction(Func<Category, TypeBase, T, Result> function, T arg)
            : base(_nextObjectId++)
        {
            _order = Closures.NextOrder++;
            _function = function;
            _arg = arg;
            Tracer.Assert(_function.Target is IContextReferenceProvider);
        }

        protected override Result Result(Category category, TypeBase argsType) => _function(category, argsType, _arg);
        protected override bool IsImplicit => false;
    }
}