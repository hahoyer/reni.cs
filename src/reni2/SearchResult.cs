using System;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Parser.TokenClass;

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

        public SearchResult<TFeatureType> Convert<TFeatureType,
                                                  TTargetType>(IConverter<TFeatureType, TTargetType> feature,
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
            : this(feature, new SearchResultDescriptor(defineable, searchTrial)) { }

        private SearchResult(TFeatureType feature, SearchResultDescriptor searchResultDescriptor)
            : this()
        {
            Feature = feature;
            SearchResultDescriptor = searchResultDescriptor;
        }

        private SearchResult(Defineable defineable, SearchTrial searchTrial)
            : this(null, defineable, searchTrial) { }

        public bool IsSuccessFull { get { return Feature != null; } }
        public SearchResultDescriptor SearchResultDescriptor { get; private set; }

        [DumpExcept(null)]
        public TFeatureType Feature { get; private set; }

        public static SearchResult<TFeatureType> Failure<TTarget>(TTarget target, Defineable defineable)
            where TTarget : IDumpShortProvider
        {
            return new SearchResult<TFeatureType>(defineable, SearchTrial.
                                                                  Create(target, Tracer.MethodHeader(1, true)));
        }

        public static SearchResult<TFeatureType> Failure(Defineable defineable) { return new SearchResult<TFeatureType>(defineable, SearchTrial.Create(Tracer.MethodHeader(1, true))); }

        public static SearchResult<TFeatureType> Success(TFeatureType feature, Defineable defineable) { return new SearchResult<TFeatureType>(feature, defineable, SearchTrial.Create(Tracer.MethodHeader(1, true))); }

        public static SearchResult<TFeatureType> SuccessIfMatch(Defineable defineable)
        {
            var x = defineable as TFeatureType;
            if(x == null) 
                return Failure(defineable);
            return Success(x, defineable);
        }

        public SearchResult<TFeatureType> AlternativeTrial(SearchResult<TFeatureType> failedResult)
        {
            var searchTrial = SearchTrial.AlternativeTrial(failedResult.SearchResultDescriptor.SearchTrial,
                                                           SearchResultDescriptor.SearchTrial,
                                                           Tracer.MethodHeader(1, true));
            return new SearchResult<TFeatureType>(Feature, SearchResultDescriptor.Defineable, searchTrial);
        }

        public SearchResult<TFeatureType> SubTrial<TTarget>(TTarget target, string reason)
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
                    Tracer.MethodHeader(1, true) + reason 
                )
            );
        }

        public static SearchResult<TFeatureType> Create(TFeatureType feature, SearchResultDescriptor descriptor)
        {
            return new SearchResult<TFeatureType>(feature, descriptor);
        }

        public static SearchResult<TFeatureType> Create<TSubFeatureType>(SearchResult<TSubFeatureType> result)
            where TSubFeatureType : class, TFeatureType
        {
            return new SearchResult<TFeatureType>(result.Feature, result.SearchResultDescriptor);
        }
    }
}