using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Sequence;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    internal abstract class SearchVisitor : ReniObject, ISearchVisitor
    {
        void ISearchVisitor.Search() { SearchTypeBase(); }
        void ISearchVisitor.ChildSearch<TType>(TType target) { InternalChild(target).Search(); }
        ISearchVisitor ISearchVisitor.Child(BaseType target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(AutomaticReferenceType target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(AccessType target) { return InternalChild(target); }
        internal abstract void SearchTypeBase();

        protected abstract ISearchVisitor InternalChild<TType>(TType target)
            where TType : IDumpShortProvider;
    }

    internal abstract class SearchVisitor<TFeature> : SearchVisitor
        where TFeature : class
    {
        private ISearchVisitor[] _children = new ISearchVisitor[0];
        private SearchResult<TFeature>[] _searchResults = new SearchResult<TFeature>[0];

        internal ISearchVisitor[] Children { get { return _children; } }
        internal SearchResult<TFeature>[] SearchResults { get { return _searchResults; } }
        internal abstract bool IsSuccessFull { get; }
        internal abstract TFeature InternalResult { set; }
        internal abstract Defineable Defineable { get; }

        internal void Search(TypeBase typeBase)
        {
            var searchResult = new SearchResult<TFeature>(this, typeBase);
            Add(searchResult);

            if(IsSuccessFull)
                return;

            searchResult.SetSearchMode();

            typeBase.Search(this);
        }

        internal override void SearchTypeBase()
        {
            var searchResult = new SearchResult<TFeature>(this);
            Add(searchResult);

            if(IsSuccessFull)
                return;

            searchResult.SetSearchMode();

            InternalResult = Defineable.Check<TFeature>();

            if(IsSuccessFull)
                searchResult.SetFoundMode();
        }

        protected override ISearchVisitor InternalChild<TType>(TType target) { return new ChildSearchVisitor<TFeature, TType>(this, target); }

        private void Add(SearchResult<TFeature> searchResult)
        {
            _searchResults = new List<SearchResult<TFeature>>(_searchResults) {searchResult}.ToArray();
        }

        internal void Add(ISearchVisitor child) { _children = new List<ISearchVisitor>(_children) {child}.ToArray(); }
    }
}