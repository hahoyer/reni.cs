using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni
{
    internal class SearchResultDescriptor: ReniObject
    {
        internal SearchResultDescriptor(Defineable defineable, SearchTrial searchTrial)
        {
            Defineable = defineable;
            SearchTrial = searchTrial;
        }
        
        public Defineable Defineable { get; private set; }
        public SearchTrial SearchTrial { get; private set; }

        public SearchResult<ISequenceFeature> Convert(ISequenceOfBitFeature inFeature)
        {
            ISequenceFeature resultFeature = null;
            if(inFeature != null)
                resultFeature = inFeature.Convert();
            return SearchResult<ISequenceFeature>.Create(resultFeature, this);
        }

        public SearchResult<ISequencePrefixFeature> Convert(ISequenceOfBitPrefixFeature inFeature)
        {
            ISequencePrefixFeature resultFeature = null;
            if (inFeature != null)
                resultFeature = inFeature.Convert();
            return SearchResult<ISequencePrefixFeature>.Create(resultFeature, this);
        }

        public SearchResult<IFeature> Convert(IRefFeature inFeature, Ref @ref)
        {
            IFeature resultFeature = null;
            if (inFeature != null)
                resultFeature = inFeature.Convert(@ref);
            return SearchResult<IFeature>.Create(resultFeature, this);
        }

        public SearchResult<IRefFeature> Convert(IRefToSequenceFeature inFeature, Sequence sequence)
        {
            IRefFeature resultFeature = null;
            if (inFeature != null)
                resultFeature = inFeature.Convert(sequence);
            return SearchResult<IRefFeature>.Create(resultFeature, this);
        }

        public SearchResult<IFeature> Convert(ISequenceFeature inFeature, Sequence sequence)
        {
            IFeature resultFeature = null;
            if (inFeature != null)
                resultFeature = inFeature.Convert(sequence);
            return SearchResult<IFeature>.Create(resultFeature, this);
        }

        public SearchResult<IPrefixFeature> Convert(ISequencePrefixFeature inFeature, Sequence sequence)
        {
            IPrefixFeature resultFeature = null;
            if (inFeature != null)
                resultFeature = inFeature.Convert(sequence);
            return SearchResult<IPrefixFeature>.Create(resultFeature, this);
        }
    }

    internal struct SearchResult<FeatureType> where FeatureType : class
    {
        private SearchResult(FeatureType feature, Defineable defineable, SearchTrial searchTrial)
            : this(feature, new SearchResultDescriptor(defineable, searchTrial))
        {
        }

        private SearchResult(FeatureType feature, SearchResultDescriptor  searchResultDescriptor)
            : this()
        {
            Feature = feature;
            SearchResultDescriptor = searchResultDescriptor;
        }

        private SearchResult(Defineable defineable, SearchTrial searchTrial)
            : this(null, defineable, searchTrial) {}

        public bool IsSuccessFull { get { return Feature != null; } }
        public SearchResultDescriptor SearchResultDescriptor { get; private set; }

        [DumpExcept(null)]
        public FeatureType Feature { get; private set; }

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

        public static SearchResult<FeatureType> Success(FeatureType feature, Defineable defineable)
        {
            return new SearchResult<FeatureType>(feature, defineable, SearchTrial.Create(Tracer.MethodHeader(1, true)));
        }

        public SearchResult<FeatureType> AlternativeTrial(SearchResult<FeatureType> failedResult)
        {
            var searchTrial = SearchTrial.AlternativeTrial(failedResult.SearchResultDescriptor.SearchTrial, SearchResultDescriptor.SearchTrial,
                Tracer.MethodHeader(1, true));
            return new SearchResult<FeatureType>(Feature, SearchResultDescriptor.Defineable, searchTrial);
        }

        public SearchResult<FeatureType> SubTrial<Target>(Target target) where Target : IDumpShortProvider
        {
            return new SearchResult<FeatureType>(Feature, SearchResultDescriptor.Defineable, SearchTrial.
                SubTrial
                (SearchResultDescriptor.SearchTrial, target, Tracer.MethodHeader(1, true)));
        }

        public static SearchResult<FeatureType> Create(FeatureType feature, SearchResultDescriptor descriptor)
        {
            return new SearchResult<FeatureType>(feature,descriptor);
        }
    }
}