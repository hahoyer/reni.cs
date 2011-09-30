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
    sealed class SearchResult: ReniObject
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

        internal Result Result(Category category, RefAlignParam refAlignParam)
        {
            var trace = ObjectId == -10 && category.HasCode;
            StartMethodDump(trace, category,refAlignParam);
            try
            {
                category = category.Typed;
                var featureResult = FeatureResult(category, refAlignParam);

                Dump("featureResult", featureResult);
                BreakExecution();

                var converterCategory = category.ReplaceArged;
                var converterResult = ConverterResult(converterCategory, refAlignParam);

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
            if(category.IsNone)
                return new Result();

            var results = _conversionFunctions.Select(f => f(category, refAlignParam)).ToArray();
            var result = results[0];
            for(var i = 1; i < results.Length; i++)
                result = result.ReplaceArg(results[i]);
            return result;
        }

        Result FeatureResult(Category category, RefAlignParam refAlignParam)
        {
            return _feature.Result(category, refAlignParam);
        }
    }
}