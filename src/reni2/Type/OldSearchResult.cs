using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.ReniSyntax;

namespace Reni.Type
{
    abstract class OldSearchResult : DumpableObject, ISearchResult
    {
        [EnableDump]
        internal readonly IFeatureImplementation Feature;

        readonly ISimpleFeature[] _conversionFunctions = new ISimpleFeature[0];

        internal OldSearchResult(IFeatureImplementation feature) { Feature = feature; }
        protected OldSearchResult(IFeatureImplementation feature, ISimpleFeature converter)
        {
            Feature = feature;
            _conversionFunctions = new[] {converter};
        }

        Result ISearchResult.SimpleResult(Category category)
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
        internal abstract TypeBase DefiningType { get; }

        [DisableDump]
        CallDescriptor CallDescriptor { get { return new CallDescriptor(DefiningType, Feature, ConverterResult); } }

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

        public Result FunctionResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return CallDescriptor.Result(category, context, left, right);
        }
    }
}