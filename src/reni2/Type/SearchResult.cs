#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using Reni.Syntax;

namespace Reni.Type
{
    abstract class SearchResult : ReniObject
    {
        static int _nextObjectId;
        [EnableDump]
        internal readonly IFeature Feature;
        [EnableDump]
        internal readonly IConversionFunction[] ConversionFunctions;

        internal SearchResult(IFeature feature, IConversionFunction[] conversionFunctions)
            : base(_nextObjectId++)
        {
            Tracer.Assert(feature != null);
            Feature = feature;
            ConversionFunctions = conversionFunctions;
        }

        internal Result Result(Category category)
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

        TypeBase LeftType
        {
            get
            {
                var result = ConverterResult(Category.Type);
                return result != null ? result.Type : DefiningType;
            }
        }

        Result ConverterResult(Category category)
        {
            var trace = ObjectId == -12 && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                if(ConversionFunctions.Length == 0)
                    return null;
                var results = ConversionFunctions
                    .Select((cf, i) => cf.Result(category.Typed))
                    .ToArray();
                Dump("results", results);
                BreakExecution();

                var result = results[0];
                for(var i = 1; i < results.Length; i++)
                    result = result.ReplaceArg(results[i]);

                return ReturnMethodDump(result.SmartLocalReferenceResult());
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result FunctionResult(ContextBase context, Category category, ExpressionSyntax syntax)
        {
            var objectDescriptor = new CallDescriptor(LeftType, Feature, ConverterResult);
            return objectDescriptor.Result(category, context, syntax.Left, syntax.Right);
        }
    }
}