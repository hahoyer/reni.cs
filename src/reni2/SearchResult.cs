using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Struct;
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

        public SearchResult<ISequenceElementFeature> Convert(ISequenceOfBitFeature feature)
        {
            ISequenceElementFeature resultFeature = null;
            if(feature != null)
                resultFeature = feature.Convert();
            return SearchResult<ISequenceElementFeature>.Create(resultFeature, this);
        }

        public SearchResult<ISequenceElementPrefixFeature> Convert(ISequenceOfBitPrefixFeature feature)
        {
            ISequenceElementPrefixFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert();
            return SearchResult<ISequenceElementPrefixFeature>.Create(resultFeature, this);
        }

        public SearchResult<IFeature> Convert(IRefFeature feature, Ref @ref)
        {
            IFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert(@ref);
            return SearchResult<IFeature>.Create(resultFeature, this);
        }

        public SearchResult<IRefFeature> Convert(IRefToSequenceFeature feature, Sequence sequence)
        {
            IRefFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert(sequence);
            return SearchResult<IRefFeature>.Create(resultFeature, this);
        }

        public SearchResult<IFeature> Convert(ISequenceElementFeature feature, Sequence sequence)
        {
            IFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert(sequence);
            return SearchResult<IFeature>.Create(resultFeature, this);
        }

        public SearchResult<IPrefixFeature> Convert(ISequenceElementPrefixFeature feature, Sequence sequence)
        {
            IPrefixFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert(sequence);
            return SearchResult<IPrefixFeature>.Create(resultFeature, this);
        }

        public SearchResult<IFeature> Convert(IFeatureForSequence feature, Sequence sequence)
        {
            IFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert(sequence);
            return SearchResult<IFeature>.Create(resultFeature, this);
        }

        public SearchResult<IContextFeature> Convert(IStructFeature feature, Struct.Context context)
        {
            IContextFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert(context);
            return SearchResult<IContextFeature>.Create(resultFeature, this);
        }

        public SearchResult<IFeature> Convert(IStructFeature feature, Struct.Type type)
        {
            IFeature resultFeature = null;
            if (feature != null)
                resultFeature = feature.Convert(type);
            return SearchResult<IFeature>.Create(resultFeature, this);
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

        public static SearchResult<FeatureType> Failure<Target>(Target target, Defineable defineable)
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
