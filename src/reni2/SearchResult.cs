using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni
{
    internal static class SearchResultExtender
    {
        internal static TFeature CheckedConvert<TFeature, TType>(this IConverter<TFeature, TType> feature, TType target)
            where TFeature : class
            where TType : IDumpShortProvider
        {
            if(feature == null)
                return null;
            return feature.Convert(target);
        }
    }

    internal interface ISearchVisitor
    {
        void SearchTypeBase();
        ISearchVisitor Child(Ref target);
        ISearchVisitor Child(AssignableRef target);
        ISearchVisitor Child(Sequence target);
        ISearchVisitor Child(Type.Array target);
        ISearchVisitor Child(Type.Void target);
    }

    internal abstract class SearchVisitor<TFeature> : ISearchVisitor
        where TFeature : class
    {
        void ISearchVisitor.SearchTypeBase() { SearchTypeBase(); }
        ISearchVisitor ISearchVisitor.Child(Ref target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(AssignableRef target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Sequence target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Type.Array target) { return InternalChild(target); }
        ISearchVisitor ISearchVisitor.Child(Type.Void target) { return InternalChild(target); }

        internal abstract bool IsSuccessFull { get; }
        internal abstract TFeature InternalResult { set; }
        internal abstract Defineable Defineable { get; }

        internal void Search(TypeBase typeBase)
        {
            if (IsSuccessFull)
                return;
            typeBase.Search(this);
        }

        internal void SearchTypeBase()
        {
            if (IsSuccessFull)
                return;

            InternalResult = Defineable.Check<TFeature>();
        }

        private ISearchVisitor InternalChild<TType>(TType target) where TType : IDumpShortProvider { return new ChildSearchVisitor<TFeature, TType>(this, target); }
    }

    internal class RootSearchVisitor<TFeature> : SearchVisitor<TFeature>
        where TFeature : class
    {
        private readonly Defineable _defineable;
        private TFeature _result;

        internal RootSearchVisitor(Defineable defineable) { _defineable = defineable; }

        internal override bool IsSuccessFull { get { return _result != null; } }

        internal override TFeature InternalResult
        {
            set
            {
                Tracer.Assert(_result == null);
                _result = value;
            }
        }

        internal TFeature Result { get { return _result; } }

        internal override Defineable Defineable { get { return _defineable; } }
    }

    internal class ContextSearchVisitor : RootSearchVisitor<IContextFeature>
    {
        internal ContextSearchVisitor(Defineable defineable)
            : base(defineable) { }

        internal void Search(ContextBase contextBase)
        {
            if(IsSuccessFull)
                return;
            contextBase.Search(this);
        }
    }

    internal class ChildSearchVisitor<TFeature, TType> : SearchVisitor<IConverter<TFeature, TType>>
        where TFeature : class
        where TType : IDumpShortProvider
    {
        private readonly SearchVisitor<TFeature> _parent;
        private readonly TType _target;

        public ChildSearchVisitor(SearchVisitor<TFeature> parent, TType target)
        {
            _parent = parent;
            _target = target;
        }

        internal override bool IsSuccessFull { get { return _parent.IsSuccessFull; } }

        internal override IConverter<TFeature, TType> InternalResult
        {
            set
            {
                if(value == null)
                    return;
                Tracer.Assert(!IsSuccessFull);
                _parent.InternalResult = value.Convert(_target);
            }
        }

        internal override Defineable Defineable { get { return _parent.Defineable; } }
    }
}