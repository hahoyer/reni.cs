#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

#endregion

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.ReniParser;

namespace Reni.Type
{
    abstract class SearchResult : ReniObject, ISearchResult
    {
        static int _nextObjectId;
        [EnableDump]
        internal readonly IFeature Feature;
        [EnableDump]
        readonly IConversionFunction[] _conversionFunctions;

        internal SearchResult(IFeature feature, IConversionFunction[] conversionFunctions)
            : base(_nextObjectId++)
        {
            Tracer.Assert(feature != null);
            Feature = feature;
            _conversionFunctions = conversionFunctions;
            StopByObjectId(-1);
        }

        Result ISearchResult.Result(Category category)
        {
            category = category.Typed;
            var featureResult = Feature.Simple.Result(category);
            if(!featureResult.HasArg)
                return featureResult;

            var converterResult = ConverterResult(category);
            var result = featureResult.ReplaceArg(converterResult);
            return result;
        }

        [DisableDump]
        protected abstract TypeBase DefiningType { get; }

        Result ConverterResult(Category category)
        {
            var trace = ObjectId == -8 && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                if(_conversionFunctions.Length == 0)
                    return null;
                var results = _conversionFunctions
                    .Select((cf, i) => cf.Result(category.Typed))
                    .ToArray();
                Dump("results", results);
                BreakExecution();

                var result = results[0];
                for(var i = 1; i < results.Length; i++)
                    result = result.ReplaceArg(results[i]);

                return ReturnMethodDump(result.LocalPointerKindResult);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result ISearchResult.FunctionResult(ContextBase context, Category category, ExpressionSyntax syntax)
        {
            var objectDescriptor = new CallDescriptor(DefiningType, Feature, ConverterResult);
            return objectDescriptor.Result(category, context, syntax.Left, syntax.Right);
        }
    }
}