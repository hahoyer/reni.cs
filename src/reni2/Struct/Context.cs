using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Context : Reni.Context.Child
    {
        private readonly int _position;
        [Node]
        internal readonly Container Container;

        private readonly SimpleCache<ContextPosition[]> _featuresCache;
        private readonly DictionaryEx<ContextBase, Field> _fieldsCache;

        [Node]
        [IsDumpEnabled(false)]
        private readonly Result[] _innerResultsCache;

        internal Context(Container container, int position)
        {
            _position = position;
            Container = container;
            _featuresCache = new SimpleCache<ContextPosition[]>(CreateFeaturesCache);
            _innerResultsCache = new Result[Container.List.Length];
            _fieldsCache = new DictionaryEx<ContextBase, Field>(parent => new Field(new StructContext(this, parent)));
        }

        internal int Position { get { return _position; } }

        [IsDumpEnabled(false)]
        internal int IndexSize { get { return Container.IndexSize; } }

        [IsDumpEnabled(false)]
        internal IReferenceInCode ForCode { get { throw new NotImplementedException(); } }

        [IsDumpEnabled(false)]
        internal ContextPosition[] Features
        {
            get
            {
                var contextPositions = _featuresCache.Value;
                AssertValid();
                return contextPositions;
            }
        }

        internal Result ContextReferenceAsArg(ContextBase context, Category category)
        {
            return ContextReferenceType(context)
                .ArgResult(category)
                .AddToReference(category, context.RefAlignParam, InternalSize(), "ContextReferenceAsArg");
        }

        internal TypeBase ContextReferenceType(ContextBase context) { throw new NotImplementedException(); }

        internal void AssertValid()
        {
            if (_featuresCache.IsValid)
                Tracer.Assert(_featuresCache.Value.Length == Position);
        }

        internal Size InnerSize(ContextBase parent) { return InnerResult(parent, Category.Size).Size; }

        internal TypeBase InnerType(ContextBase parent) { return InnerResult(parent, Category.Type).Type; }

        internal ISearchPath<IFeature, Type> SearchFromRefToStruct(Defineable defineable) { throw new NotImplementedException(); }

        internal TypeBase InnerType(ContextBase parent, int position)
        {
            throw new NotImplementedException();
        }

        private Result InnerResult(ContextBase parent, Category category)
        {
            return InnerResult(category, parent, 0, Position);
        }

        private ContextPosition[] CreateFeaturesCache()
        {
            var result = new List<ContextPosition>();
            for (var i = 0; i < Position; i++)
                result.Add(new ContextPosition(this, i));
            return result.ToArray();
        }

        private Result InnerResult(Category category, ContextBase parent, int position)
        {
            //Tracer.ConditionalBreak(Container.ObjectId == 0 && position == 0, ()=>"");
            var result = Container.InternalResultForStruct(category, parent, position);
            Tracer.Assert(!(category.HasType && result.Type is Reference));
            if (_innerResultsCache[position] == null)
                _innerResultsCache[position] = new Result();
            _innerResultsCache[position].Update(result);
            return result;
        }

        private Result InnerResult(Category category, ContextBase parent, int fromPosition, int fromNotPosition)
        {
            var result = TypeBase.VoidResult(category);
            for (var i = fromPosition; i < fromNotPosition; i++)
                result = result.CreateSequence(InnerResult(category, parent, i));
            return result;
        }

        internal Field SpawnField(ContextBase parent) { return _fieldsCache.Find(parent); }
    }
}