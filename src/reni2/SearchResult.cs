using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Struct;

namespace Reni
{
    internal class SearchResultDescriptor : ReniObject
    {
        internal SearchResultDescriptor(Defineable defineable, SearchTrial searchTrial)
        {
            Defineable = defineable;
            SearchTrial = searchTrial;
        }

        public Defineable Defineable { get; private set; }
        public SearchTrial SearchTrial { get; private set; }

        public SearchResult<TFeatureType> Convert<TFeatureType,TTargetType>
            (
            IConverter<TFeatureType, TTargetType> feature,
            TTargetType target)
            where TFeatureType : class
        {
            TFeatureType resultFeature = null;
            if(feature != null)
                resultFeature = feature.Convert(target);
            return SearchResult<TFeatureType>.Create(resultFeature, this);
        }
    }

    internal struct SearchResult<TFeatureType>
        where TFeatureType : class
    {
        private SearchResult(TFeatureType feature, Defineable defineable, SearchTrial searchTrial)
            : this(feature, new SearchResultDescriptor(defineable, searchTrial))
        {
        }

        private SearchResult(TFeatureType feature, SearchResultDescriptor searchResultDescriptor)
            : this()
        {
            Feature = feature;
            SearchResultDescriptor = searchResultDescriptor;
        }

        public SearchResult(TFeatureType feature, Defineable defineable)
            : this(feature, defineable, SearchTrial.Create(Tracer.MethodHeader(1, true)))
        {                               
        }

        public bool IsSuccessFull { get { return Feature != null; } }
        public SearchResultDescriptor SearchResultDescriptor { get; private set; }

        [DumpExcept(null)]
        public TFeatureType Feature { get; private set; }

        public static SearchResult<TFeatureType> Create(TFeatureType feature, Defineable defineable)
        {
            return new SearchResult<TFeatureType>(feature, defineable, SearchTrial.Create(Tracer.MethodHeader(1, true)));
        }

        public SearchResult<TFeatureType> RecordAlternativeTrial(SearchResult<TFeatureType> failedResult)
        {
            var searchTrial = SearchTrial.AlternativeTrial(failedResult.SearchResultDescriptor.SearchTrial,
                                                           SearchResultDescriptor.SearchTrial,
                                                           Tracer.MethodHeader(1, true));
            return new SearchResult<TFeatureType>(Feature, SearchResultDescriptor.Defineable, searchTrial);
        }

        public SearchResult<TFeatureType> RecordSubTrial<TTarget>(TTarget target)
            where TTarget : IDumpShortProvider

        {
            return new SearchResult<TFeatureType>
                (
                Feature,
                SearchResultDescriptor.Defineable,
                SearchTrial.SubTrial
                    (
                    SearchResultDescriptor.SearchTrial,
                    target,
                    Tracer.MethodHeader(1, true)
                    )
                );
        }

        public static SearchResult<TFeatureType> Create(TFeatureType feature, SearchResultDescriptor descriptor)
        {
            return new SearchResult<TFeatureType>(feature, descriptor);
        }

        public static SearchResult<TFeatureType> Convert<TSubFeatureType>(SearchResult<TSubFeatureType> result)
            where TSubFeatureType : class, TFeatureType
        {
            return Create(result.Feature, result.SearchResultDescriptor);
        }

        internal SearchResult<TFeatureType> Or(Func<SearchResult<TFeatureType>> alternative)
        {
            if(IsSuccessFull)
                return this;
            return alternative().RecordAlternativeTrial(this);
        }
    }

    internal static class SearchResultExtender
    {
        internal static SearchResult<TFeature> Convert<TFeature, TType>(this SearchResult<IConverter<TFeature, TType>> containerResult, TType target)
            where TFeature : class
        {
            return containerResult.SearchResultDescriptor.Convert(containerResult.Feature, target);
        }

    }
}