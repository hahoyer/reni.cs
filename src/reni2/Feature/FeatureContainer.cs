using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Feature
{
    abstract class FeatureContainer : DumpableObject
    {
        protected FeatureContainer(IFeatureImplementation feature) { Feature = feature; }

        internal  IFeatureImplementation Feature { get; }
    }
}