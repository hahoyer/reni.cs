using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ContainerContext : ReniObject, IDumpShortProvider
    {
        private readonly Container _container;
        private readonly ContextBase _parent;
        private readonly DictionaryEx<int, PositionContainerContext> _positionContainerContextsCache;

        internal ContainerContext(Container container, ContextBase parent)
        {
            _container = container;
            _parent = parent;
            _positionContainerContextsCache = new DictionaryEx<int, PositionContainerContext>(position => new PositionContainerContext(this, position));
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        internal Container Container { get { return _container; } }
        internal ContextBase Parent { get { return _parent; } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return _parent.RefAlignParam; } }

        [DisableDump]
        internal int IndexSize { get { return _container.IndexSize; } }

        [DisableDump]
        internal Root RootContext { get { return _parent.RootContext; } }

        internal PositionContainerContext SpawnPositionContainerContext(int position) { return _positionContainerContextsCache.Find(position); }
        internal ContextBase SpawnContext(int position) { return _container.SpawnContext(_parent, position); }
        internal Size InnerSize(int position) { return _container.InnerSize(_parent, position); }
        internal TypeBase InnerType(int position) { return _container.InnerType(_parent, position); }
        internal Size InnerOffset(int position) { return Container.InnerResult(Category.Size, Parent, 0, position).Size; }
    }
}