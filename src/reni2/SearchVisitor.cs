using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using JetBrains.Annotations;
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    internal abstract class SearchVisitor : ReniObject, ISearchVisitor
    {
        void ISearchVisitor.Search() { SearchTypeBase(); }
        void ISearchVisitor.ChildSearch<TType>(TType target) { InternalChild(target).Search(); }
        ISearchVisitor ISearchVisitor.Child(BaseType target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Reference target) { return InternalChild(target); }
        internal abstract void SearchTypeBase();
        protected abstract ISearchVisitor InternalChild<TType>(TType target) where TType : IDumpShortProvider;
    }

    internal abstract class SearchVisitor<TFeature> : SearchVisitor
        where TFeature : class
    {
        private readonly List<ISearchVisitor> _children = new List<ISearchVisitor>();
        private readonly List<SearchResult<TFeature>> _searchResults = new List<SearchResult<TFeature>>();

        internal abstract bool IsSuccessFull { get; }
        internal abstract TFeature InternalResult { set; }
        internal abstract Defineable Defineable { get; }

        internal void Search(TypeBase typeBase)
        {
            var searchResult = new SearchResult<TFeature>(typeBase, this);
            _searchResults.Add(searchResult);

            if(IsSuccessFull)
                return;

            searchResult.SetSearchMode();

            typeBase.Search(this);
        }

        internal override void SearchTypeBase()
        {
            var searchResult = new SearchResult<TFeature>(null, this);
            _searchResults.Add(searchResult);

            if(IsSuccessFull)
                return;

            searchResult.SetSearchMode();

            InternalResult = Defineable.Check<TFeature>();

            if(IsSuccessFull)
                searchResult.SetFoundMode();
        }

        protected override ISearchVisitor InternalChild<TType>(TType target) { return new ChildSearchVisitor<TFeature, TType>(this, target); }

        public void AddChild(ISearchVisitor child) { _children.Add(child); }
    }

    internal sealed class SearchResult<TFeature> : ReniObject
        where TFeature : class
    {
        [UsedImplicitly]
        private readonly TypeBase _typeBase;

        [UsedImplicitly]
        private readonly SearchVisitor<TFeature> _searchVisitor;

        [UsedImplicitly]
        private bool? _isFoundMode;

        public SearchResult(TypeBase typeBase, SearchVisitor<TFeature> searchVisitor)
        {
            _typeBase = typeBase;
            _searchVisitor = searchVisitor;
        }

        public void SetSearchMode() { _isFoundMode = false; }
        public void SetFoundMode() { _isFoundMode = true; }
    }
}