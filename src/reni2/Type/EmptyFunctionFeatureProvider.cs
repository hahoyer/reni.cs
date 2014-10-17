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
        IContextMetaFunctionFeature IFeatureImplementation.ContextMetaFunction { get { return null; } }
        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }
    }
}