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
        readonly Func<Category, IContextReference, TypeBase, Result> Function;
        readonly IContextReferenceProvider Target;
        static int NextObjectId;
        [UsedImplicitly]
        readonly int Order;

        protected ObjectFunctionBase
            (Func<Category, IContextReference, TypeBase, Result> function, IContextReferenceProvider target)
            : base(NextObjectId++)
        {
            Order = Closures.NextOrder++;
            Function = function;
            Target = target;
        }

        Result IFunction.Result(Category category, TypeBase argsType)
            => Function(category, ObjectReference, argsType);

        bool IFunction.IsImplicit => false;
        IContextReference ObjectReference => Target.ContextReference;
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
        readonly Func<Category, TypeBase, Result> Data;

        internal Function(Func<Category, TypeBase, Result> function) { Data = function; }

        protected override Result Result(Category category, TypeBase argsType) => Data(category, argsType);
        protected override bool IsImplicit => false;
    }

    sealed class ExtendedFunction<T> : FunctionFeatureImplementation
    {
        static int NextObjectId;
        [UsedImplicitly]
        readonly int Order;

        readonly Func<Category, TypeBase, T, Result> Function;
        readonly T Argument;

        public ExtendedFunction(Func<Category, TypeBase, T, Result> function, T argument)
            : base(NextObjectId++)
        {
            Order = Closures.NextOrder++;
            Function = function;
            Argument = argument;
            (Function.Target is IContextReferenceProvider).Assert();
        }

        protected override Result Result(Category category, TypeBase argsType) => Function(category, argsType, Argument);
        protected override bool IsImplicit => false;
    }
}