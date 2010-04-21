using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reni.Parser.TokenClass;
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    internal abstract class SearchVisitor<TFeature> : ReniObject, ISearchVisitor
        where TFeature : class
    {
        private readonly List<ISearchVisitor> _children = new List<ISearchVisitor>();
        private readonly List<SearchResult<TFeature>> _searchResults = new List<SearchResult<TFeature>>();
        void ISearchVisitor.Search() { SearchTypeBase(); }
        ISearchVisitor ISearchVisitor.Child(Bit target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Type.Reference target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Struct.Reference target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Sequence target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Type.Array target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Type.Void target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Struct.Type target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(TypeType target) { return InternalChild(target); }

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

        internal void SearchTypeBase()
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

        private ISearchVisitor InternalChild<TType>(TType target) where TType : IDumpShortProvider { return new ChildSearchVisitor<TFeature, TType>(this, target); }

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