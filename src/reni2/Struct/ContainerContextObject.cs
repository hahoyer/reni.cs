using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    sealed class ContainerContextObject : DumpableObject, IContextReference
    {
        static int _nextObjectId;
        readonly Container _container;
        readonly ContextBase _parent;
        readonly int _order;

        internal ContainerContextObject(Container container, ContextBase parent)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _container = container;
            _parent = parent;
        }

        int IContextReference.Order { get { return _order; } }
        Size IContextReference.Size { get { return RefAlignParam.RefSize; } }
        [Node]
        internal Container Container { get { return _container; } }
        [Node]
        internal ContextBase Parent { get { return _parent; } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return Root.DefaultRefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return _parent.RootContext.BitType.UniqueNumber(IndexSize.ToInt()); } }

        [DisableDump]
        internal Root RootContext { get { return Parent.RootContext; } }

        [DisableDump]
        Structure ToStructure { get { return _parent.UniqueStructure(Container); } }

        Size IndexSize { get { return Container.IndexSize; } }

        internal TypeBase AccessType(int accessPosition, int position)
        {
            return Container.AccessType(_parent, accessPosition, position);
        }

        internal Size StructureSize(int position)
        {
            if(StructureHllw(position))
                return Size.Zero;
            return StructureSize(0, position);
        }

        internal bool StructureHllw(int accessPosition) { return Container.ObtainHllw(Parent, accessPosition); }

        internal Result ContextReferenceViaStructReference(int position, Result result)
        {
            return result.ReplaceAbsolute(this, () => ContextReferenceViaStructReferenceCode(position), CodeArgs.Arg);
        }

        internal Result Result(Category category, Result innerResult)
        {
            var result = innerResult.ReplaceRelative(this, CodeBase.TopRef, CodeArgs.Void);
            if(category.HasType)
                result.Type = ToStructure.Type;
            return result;
        }

        internal Size ContextReferenceOffsetFromAccessPoint(int position) { return StructureSize(0, position); }

        Size StructureSize(int fromPosition, int fromNotPosition)
        {
            return Container.StructureSize(Parent, fromPosition, fromNotPosition);
        }

        CodeBase ContextReferenceViaStructReferenceCode(int accessPosition)
        {
            return Parent
                .UniqueStructure(Container, accessPosition)
                .PointerKind.ArgCode
                .ReferencePlus(ContextReferenceOffsetFromAccessPoint(accessPosition));
        }

        internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition)
        {
            return Container.StructureSize(Parent, fieldPosition + 1, accessPosition);
        }
    }
}