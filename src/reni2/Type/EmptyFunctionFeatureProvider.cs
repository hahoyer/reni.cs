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
        IContextMetaFunctionFeature IContextMetaFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IMetaFeatureImplementation.Meta => null;
        IFunctionFeature ITypedFeatureImplementation.Function => null;
        IValueFeature ITypedFeatureImplementation.Value => null;
    }
}