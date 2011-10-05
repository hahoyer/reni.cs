//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    abstract class SearchResult : ReniObject
    {
        static int _nextObjectId;
        [EnableDump]
        readonly IFeature _feature;
        [EnableDump]
        readonly ConversionFunction[] _conversionFunctions;
        internal SearchResult(IFeature feature, ConversionFunction[] conversionFunctions)
            : base(_nextObjectId++)
        {
            Tracer.Assert(feature != null);
            _feature = feature;
            _conversionFunctions = conversionFunctions;
        }

        protected ConversionFunction[] ConversionFunctions { get { return _conversionFunctions; } }

        internal Result Result(Category category, RefAlignParam refAlignParam)
        {
            var trace = ObjectId == -10 && category.HasCode;
            StartMethodDump(trace, category, refAlignParam);
            try
            {
                category = category.Typed;
                var featureResult = FeatureResult(category, refAlignParam);
                if(!featureResult.HasArg || ConversionFunctions.Length == 0)
                    return ReturnMethodDump(featureResult, true);

                Dump("featureResult", featureResult);
                BreakExecution();

                var converterResult = ConverterResult(category).SmartLocalReferenceResult(refAlignParam);

                Dump("converterResult", converterResult);
                BreakExecution();

                var result = featureResult.ReplaceArg(converterResult);

                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result ConverterResult(Category category, RefAlignParam refAlignParam)
        {
            if (ConversionFunctions.Length == 0)
                return TrivialConversionResult(category, refAlignParam);
            return ConverterResult(category.Typed).SmartLocalReferenceResult(refAlignParam);
        }

        protected abstract Result TrivialConversionResult(Category category, RefAlignParam refAlignParam);
        protected abstract Result ConverterResult(Category category);

        Result FeatureResult(Category category, RefAlignParam refAlignParam) { return _feature.Result(category, refAlignParam); }
    }
}