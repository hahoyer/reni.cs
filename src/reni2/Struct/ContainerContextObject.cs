using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ContainerContextObject : ReniObject, IDumpShortProvider, IReferenceInCode
    {
        private readonly Container _container;
        private readonly ContextBase _parent;
        private readonly DictionaryEx<int, AccessManager.IAccessObject> _accessObjectsCache;

        internal ContainerContextObject(Container container, ContextBase parent)
        {
            _container = container;
            _parent = parent;
            _accessObjectsCache = new DictionaryEx<int, AccessManager.IAccessObject>(GetAccessObject);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        [DisableDump]
        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }

        internal Container Container { get { return _container; } }
        internal ContextBase Parent { get { return _parent; } }

        [DisableDump]
        internal ICompileSyntax[] Statements { get { return Container.Statements; } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return TypeBase.Number(IndexSize); } }

        [DisableDump]
        internal Root RootContext { get { return Parent.RootContext; } }

        [DisableDump]
        internal Structure ToStructure { get { return _parent.SpawnStructure(Container); } }

        private int IndexSize { get { return Container.IndexSize; } }

        internal AccessManager.IAccessObject SpawnAccessObject(int position) { return _accessObjectsCache.Find(position); }
        internal Size InnerSize(int position) { return Container.InnerSize(_parent, position); }
        internal TypeBase InnerType(int position) { return Container.InnerType(_parent, position); }
        internal Size StructSize(int position) { return Container.ConstructionResult(Category.Size, Parent, 0, position).Size; }
        internal Result AccessFromContextReference(Category category, AccessType typeBase, int endPosition)
        {
            var result = typeBase
                .Result
                (category
                 , () => AccessPointCodeFromContextReference(endPosition)
                 , () => Refs.Create(this)
                );
            return result;
        }

        internal Result ReplaceContextReferenceByThisReference(int position, Result result) { return result.ReplaceAbsolute(this, () => ReplaceContextReferenceByThisReferenceCode(position), Refs.None); }

        internal Result Result(Category category, Result innerResult)
        {
            var result = innerResult.ReplaceRelative(this, () => CodeBase.TopRef(RefAlignParam), Refs.None);
            if(category.HasType)
                result.Type = ToStructure.Type;
            return result;
        }

        private Size FieldOffsetFromContextReference(int position)
        {
            return Container
                       .ConstructionResult(Category.Size, Parent, 0, position + 1).Size*-1;
        }

        private Size ContextReferenceOffsetFromAccessPoint(int position)
        {
            return Container
                .ConstructionResult(Category.Size, Parent, 0, position).Size;
        }
        private bool IsLambda(int position) { return Container.IsLambda(position); }
        private bool IsPoperty(int position) { return Container.IsProperty(position); }

        private AccessManager.IAccessObject GetAccessObject(int position)
        {
            if(IsLambda(position))
            {
                if(IsPoperty(position))
                    return AccessManager.Property;
                return AccessManager.Function;
            }
            if(InnerSize(position).IsZero)
                return AccessManager.ProcedureCall;
            return AccessManager.Field;
        }

        internal CodeBase AccessPointCodeFromContextReference(int endPosition)
        {
            return CodeBase
                .ReferenceCode(this)
                .AddToReference(RefAlignParam, ContextReferenceOffsetFromAccessPoint(endPosition)* -1);
        }

        private CodeBase ReplaceContextReferenceByThisReferenceCode(int accessPosition)
        {
            return CodeBase
                .Arg(Parent.SpawnStructure(Container).StructureReferenceType)
                .AddToReference(RefAlignParam, ContextReferenceOffsetFromAccessPoint(accessPosition));
        }

        internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition) { return Container.ConstructionSize(Parent, fieldPosition + 1, accessPosition); }
    }
}