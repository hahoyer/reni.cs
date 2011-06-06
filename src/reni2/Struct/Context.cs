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

        public Context(Container container, int position)
        {
            _position = position;
            Container = container;
            _featuresCache = new SimpleCache<ContextPosition[]>(CreateFeaturesCache);
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

        internal Size Size(ContextBase container) { return Result(container, Category.Size).Size; }

        internal TypeBase Type(ContextBase container) { return Result(container, Category.Type).Type; }

        internal Result Result(ContextBase container, Category category)
        {
            NotImplementedMethod(container,category);
            return null;
        }

        internal ISearchPath<IFeature, Type> SearchFromRefToStruct(Defineable defineable) { throw new NotImplementedException(); }

        private ContextPosition[] CreateFeaturesCache()
        {
            var result = new List<ContextPosition>();
            for (var i = 0; i < Position; i++)
                result.Add(new ContextPosition(this, i));
            return result.ToArray();
        }

    }
}