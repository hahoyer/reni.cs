﻿using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    internal sealed class ConversionFeature : IFunctionalFeature
    {
        private readonly TypeBase _typeBase;

        public ConversionFeature(TypeBase typeBase) { _typeBase = typeBase; }

        string IDumpShortProvider.DumpShort() { return _typeBase.DumpShort() + " type"; }
        Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }
        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return argsType.Conversion(category, _typeBase); }
    }
}