using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Feature
{
    abstract class FunctionBase : DumpableObject, IFunctionFeature
    {
        readonly Func<Category, IContextReference, TypeBase, Result> _function;
        static int _nextObjectId;
        [UsedImplicitly]
        readonly int _order;

        protected FunctionBase(Func<Category, IContextReference, TypeBase, Result> function)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _function = function;
            Tracer.Assert(_function.Target is IContextReferenceProvider);
        }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
            => _function(category, ObjectReference, argsType);

        bool IFunctionFeature.IsImplicit => false;
        IContextReference IFunctionFeature.ObjectReference => ObjectReference;
        IContextReference ObjectReference => ((IContextReferenceProvider) _function.Target).ContextReference;
    }

    sealed class Function : FunctionBase, IFeatureImplementation
    {
        public Function(Func<Category, IContextReference, TypeBase, Result> function)
            : base(function) {}

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => this;
        ISimpleFeature IFeatureImplementation.Simple => null;
    }

    sealed class ExtendedFunction<T> : DumpableObject, IFunctionFeature, IFeatureImplementation
    {
        static int _nextObjectId;
        [UsedImplicitly]
        readonly int _order;

        readonly Func<Category, TypeBase, T, Result> _function;
        readonly T _arg;

        public ExtendedFunction(Func<Category, TypeBase, T, Result> function, T arg)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _function = function;
            _arg = arg;
            Tracer.Assert(_function.Target is IContextReferenceProvider);
        }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => this;
        ISimpleFeature IFeatureImplementation.Simple => null;

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) => _function(category, argsType, _arg);

        bool IFunctionFeature.IsImplicit => false;
        IContextReference IFunctionFeature.ObjectReference => ObjectReference;
        IContextReference ObjectReference => ((IContextReferenceProvider) _function.Target).ContextReference;
    }
}