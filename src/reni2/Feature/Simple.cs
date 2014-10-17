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
        static int _nextObjectId;
        protected SimpleBase(Func<Category, Result> function)
            : base(_nextObjectId++)
        {
            _function = function;
        }
        Result ISimpleFeature.Result(Category category) { return _function(category); }
    }

    sealed class Simple : SimpleBase, IFeatureImplementation
    {
        readonly Func<Category, Result> _function;
        public Simple(Func<Category, Result> function)
            : base(function) { _function = function; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMetaFunction { get { return null; } }
        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return this; } }
    }

    sealed class Simple<TType>
        : DumpableObject
            , IPath<IFeatureImplementation, TType>
        where TType : TypeBase
    {
        readonly Func<Category, TType, Result> _function;

        public Simple(Func<Category, TType, Result> function) { _function = function; }
        IFeatureImplementation IPath<IFeatureImplementation, TType>.Convert(TType provider)
        {
            return new Simple(category => _function(category, provider));
        }
    }

    sealed class Simple<TType1, TType2>
        : DumpableObject
            , IPath<IPath<IFeatureImplementation, TType1>, TType2>
            , IFeatureImplementation
        where TType2 : TypeBase
        where TType1 : TypeBase
    {
        readonly Func<Category, TType1, TType2, Result> _function;
        internal Simple(Func<Category, TType1, TType2, Result> function) { _function = function; }
        IPath<IFeatureImplementation, TType1> IPath<IPath<IFeatureImplementation, TType1>, TType2>.Convert(TType2 provider)
        {
            return new Simple<TType1>((category, type1) => _function(category, type1, provider));
        }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMetaFunction { get { return null; } }
        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
    }
}