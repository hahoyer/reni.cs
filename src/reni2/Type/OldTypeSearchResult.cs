using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Type
{
    sealed class OldTypeSearchResult<TPath> : DumpableObject, ISearchResult
    {
        [EnableDump]
        readonly TypeBase _type;
        [EnableDump]
        readonly TPath _feature;

        internal OldTypeSearchResult(TypeBase type, TPath feature)
        {
            _type = type;
            _feature = feature;
        }

        Result ISearchResult.FunctionResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return CallDescriptor.Result(category, context, left, right);
        }
        Result ISearchResult.SimpleResult(Category category) { return Feature.Simple.Result(category); }

        CallDescriptor CallDescriptor { get { return new CallDescriptor(_type, Feature, category => null); } }

        IFeatureImplementation Feature
        {
            get
            {
                var result = _feature as IFeatureImplementation;
                if(result != null)
                    return result;
                NotImplementedMethod();
                return null;
            }
        }

    }

    sealed class SearchResultWithConversion : DumpableObject, ISearchResult
    {
        [EnableDump]
        readonly ISearchResult _searchResult;
        [EnableDump]
        readonly ISimpleFeature _converter;

        internal SearchResultWithConversion(ISearchResult searchResult, ISimpleFeature converter)
        {
            _searchResult = searchResult;
            _converter = converter;
        }
        Result ISearchResult.FunctionResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            NotImplementedMethod(context, category, left, right);
            return null;
        }
        Result ISearchResult.SimpleResult(Category category)
        {
            var trace = true;
            StartMethodDump(trace, category);
            try
            {
                var parent = _searchResult.SimpleResult(category);
                Dump("parent", parent);
                var conversion = _converter.Result(category);
                Dump("conversion ", conversion);
                BreakExecution();
                return null;
            }
            finally
            {
                EndMethodDump();
            }
        }

    }
}