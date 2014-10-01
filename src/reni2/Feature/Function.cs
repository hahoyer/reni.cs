using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    abstract class FunctionBase : DumpableObject, IFunctionFeature
    {
        readonly Func<Category, IContextReference, TypeBase, Result> _function;
        static int _nextObjectId;
        readonly int _order;

        protected FunctionBase(Func<Category, IContextReference, TypeBase, Result> function)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _function = function;
            Tracer.Assert(_function.Target is IContextReferenceProvider);
        }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            return _function(category, ObjectReference, argsType);
        }

        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }
        IContextReference ObjectReference { get { return ((IContextReferenceProvider) _function.Target).ContextReference; } }
    }

    sealed class Function : FunctionBase, IFeatureImplementation
    {
        public Function(Func<Category, IContextReference, TypeBase, Result> function)
            : base(function)
        {}

        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return this; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }
    }

    sealed class ExtendedFunction<T> : DumpableObject, IFunctionFeature, IFeatureImplementation
    {
        static int _nextObjectId;
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

        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return this; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            return _function(category, argsType, _arg);
        }

        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }
        IContextReference ObjectReference { get { return ((IContextReferenceProvider) _function.Target).ContextReference; } }
    }
}