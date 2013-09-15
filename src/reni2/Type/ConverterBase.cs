using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    abstract class ConverterBase : DumpableObject, IFeatureImplementation, ISimpleFeature
    {
        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return this; } }
        Result ISimpleFeature.Result(Category category) { return Result(category); }
        protected abstract Result Result(Category category);
    }
}