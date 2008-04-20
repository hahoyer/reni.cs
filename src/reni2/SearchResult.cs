using HWClassLibrary.Debug;
using Reni.Parser.TokenClass;

namespace Reni
{
    internal struct SearchResult<FeatureType> where FeatureType : class
    {
        private SearchResult(FeatureType feature, Defineable defineable, SearchTrial searchTrial)
            : this()
        {
            Feature = feature;
            Defineable = defineable;
            SearchTrial = searchTrial;
        }

        private SearchResult(Defineable defineable, SearchTrial searchTrial)
            : this(null, defineable, searchTrial) {}

        public bool IsSuccessFull { get { return Feature != null; } }
        public Defineable Defineable { get; private set; }

        [DumpExcept(null)]
        public FeatureType Feature { get; private set; }

        public SearchTrial SearchTrial { get; private set; }

        public static SearchResult<FeatureType> Failure<Target>
            (Target target, Defineable defineable) 
            where Target : IDumpShortProvider
        {
            return new SearchResult<FeatureType>(defineable, SearchTrial.
                Create(target, Tracer.MethodHeader(1, true)));
        }

        public static SearchResult<FeatureType> Failure(Defineable defineable)
        {
            return new SearchResult<FeatureType>(defineable, SearchTrial.Create(Tracer.MethodHeader(1, true)));
        }

        public static SearchResult<FeatureType> Success(FeatureType result, Defineable defineable)
        {
            return new SearchResult<FeatureType>(result, defineable, SearchTrial.Create(Tracer.MethodHeader(1, true)));
        }

        public SearchResult<FeatureType> AlternativeTrial(SearchResult<FeatureType> failedResult)
        {
            var searchTrial = SearchTrial.AlternativeTrial(failedResult.SearchTrial, SearchTrial,
                Tracer.MethodHeader(1, true));
            return new SearchResult<FeatureType>(Feature, Defineable, searchTrial);
        }

        public SearchResult<FeatureType> SubTrial<Target>(Target target) where Target : IDumpShortProvider
        {
            return new SearchResult<FeatureType>(Feature, Defineable, SearchTrial.
                SubTrial
                (SearchTrial, target, Tracer.MethodHeader(1, true)));
        }
    }
}