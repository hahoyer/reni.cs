using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ContainerContextObject : ReniObject, IDumpShortProvider, IReferenceInCode
    {
        private readonly Container _container;
        private readonly ContextBase _parent;
        private readonly DictionaryEx<int, AccessPoint> _accessPointsCache;
        private readonly DictionaryEx<int, AccessManager.IAccessObject> _accessObjectsCache;

        internal ContainerContextObject(Container container, ContextBase parent)
        {
            _container = container;
            _parent = parent;
            _accessPointsCache = new DictionaryEx<int, AccessPoint>(position => new AccessPoint(this, position));
            _accessObjectsCache = new DictionaryEx<int, AccessManager.IAccessObject>(GetAccessObject);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }
        bool IReferenceInCode.IsChildOf(ContextBase contextBase) { return _parent.IsChildOf(contextBase); }

        [DisableDump]
        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }

        internal Container Container { get { return _container; } }
        internal ContextBase Parent { get { return _parent; } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return _parent.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return TypeBase.Number(IndexSize); } }

        [DisableDump]
        internal Root RootContext { get { return _parent.RootContext; } }

        [DisableDump]
        internal AccessPoint ToAccessPoint { get { return SpawnAccessPoint(_container.EndPosition); } }

        private int IndexSize { get { return _container.IndexSize; } }

        internal AccessPoint SpawnAccessPoint(int position) { return _accessPointsCache.Find(position); }
        internal ContextBase SpawnContext(int position) { return _container.SpawnContext(_parent, position); }
        internal AccessManager.IAccessObject SpawnAccessObject(int position) { return _accessObjectsCache.Find(position); }
        internal Size InnerSize(int position) { return _container.InnerSize(_parent, position); }
        internal TypeBase InnerType(int position) { return _container.InnerType(_parent, position); }
        internal Size InnerOffset(int position) { return Container.InnerResult(Category.Size, Parent, 0, position).Size; }
        private Size InnerOffset(int accessPosition, int position) { return Container.InnerResult(Category.Size, Parent, position + 1, accessPosition).Size; }
        private bool IsLambda(int position) { return Container.IsLambda(position); }
        private bool IsPoperty(int position) { return Container.IsProperty(position); }

        private AccessManager.IAccessObject GetAccessObject(int position)
        {
            if(IsLambda(position))
            {
                if (IsPoperty(position))
                    return AccessManager.Property;
                return AccessManager.Function;
            }
            if(InnerSize(position).IsZero)
                return AccessManager.ProcedureCall;
            return AccessManager.Field;
        }

        internal Result FieldAccessFromThisReference(Category category, int accessPosition, int position)
        {
            var result = new Result
                (category
                 , () => RefAlignParam.RefSize
                 , () => InnerType(position).Reference(RefAlignParam)
                 , () => CodeBase.Arg(RefAlignParam.RefSize).AddToReference(RefAlignParam, InnerOffset(accessPosition, position), "FieldAccessFromThisReference")
                 , () => Container.InnerResult(Category.Refs, Parent, position).Refs
                );
            return result;
        }

        internal Result Result(Category category, Result innerResult)
        {
            var result = innerResult.ReplaceRelative(this, () => CodeBase.TopRef(RefAlignParam));
            if (category.HasType)
                result.Type = ToAccessPoint.Type;
            return result;
        }
    }
}