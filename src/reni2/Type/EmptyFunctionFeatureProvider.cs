using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Feature;

namespace Reni.Type
{
    sealed class EmptyFeature : DumpableObject, IFeatureImplementation
    {
        internal static readonly IFeatureImplementation Instance = new EmptyFeature();
        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => null;
        IValueFeature IFeatureImplementation.Value => null;
    }
}