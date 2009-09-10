using System;
using System.Linq;
using System.Collections.Generic;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni
{
    internal abstract class SearchVisitor<TFeature> : ISearchVisitor
        where TFeature : class
    {
        void ISearchVisitor.Search() { SearchTypeBase(); }
        ISearchVisitor ISearchVisitor.Child(Bit target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Ref target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(AssignableRef target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Sequence target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Type.Array target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Type.Void target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Struct.Type target) { return InternalChild(target); }

        internal abstract bool IsSuccessFull { get; }
        internal abstract TFeature InternalResult { set; }
        internal abstract Defineable Defineable { get; }

        internal void Search(TypeBase typeBase)
        {
            if(IsSuccessFull)
                return;
            typeBase.Search(this);
        }

        internal void SearchTypeBase()
        {
            if(IsSuccessFull)
                return;

            InternalResult = Defineable.Check<TFeature>();
        }

        private ISearchVisitor InternalChild<TType>(TType target) where TType : IDumpShortProvider { return new ChildSearchVisitor<TFeature, TType>(this, target); }
    }
}