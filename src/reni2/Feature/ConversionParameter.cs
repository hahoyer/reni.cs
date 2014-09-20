using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Feature
{
    sealed class ConversionParameter : DumpableObject, IConversionParameter
    {
        static readonly IConversionParameter _instanceWithEnableCut = new ConversionParameter {_enableCut = true};
        public static readonly IConversionParameter Instance = new ConversionParameter();
        bool _enableCut;

        ConversionParameter() { }

        IConversionParameter IConversionParameter.EnsureEnableCut { get { return _instanceWithEnableCut; } }
        bool IConversionParameter.EnableCut { get { return _enableCut; } }
    }
}