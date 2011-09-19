//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Sequence;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    abstract class SearchVisitor : ReniObject, ISearchVisitor
    {
        void ISearchVisitor.Search() { SearchTypeBase(); }
        void ISearchVisitor.ChildSearch<TType>(TType target) { InternalChild(target).Search(); }
        ISearchVisitor ISearchVisitor.Child(SequenceType target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(AutomaticReferenceType target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(AccessType target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(TextItemType target) { return InternalChild(target); }
        internal abstract void SearchTypeBase();

        protected abstract ISearchVisitor InternalChild<TType>(TType target)
            where TType : IDumpShortProvider;
    }

    abstract class SearchVisitor<TFeature> : SearchVisitor
        where TFeature : class
    {
        ISearchVisitor[] _children = new ISearchVisitor[0];
        SearchResult<TFeature>[] _searchResults = new SearchResult<TFeature>[0];

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
        Result SearchTypeBaseConverter(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        protected override ISearchVisitor InternalChild<TType>(TType target) { return new ChildSearchVisitor<TFeature, TType>(this, target); }

        void Add(SearchResult<TFeature> searchResult) { _searchResults = new List<SearchResult<TFeature>>(_searchResults) {searchResult}.ToArray(); }

        internal void Add(ISearchVisitor child) { _children = new List<ISearchVisitor>(_children) {child}.ToArray(); }
    }
}