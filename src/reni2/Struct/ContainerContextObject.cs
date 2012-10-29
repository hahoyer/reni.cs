#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    sealed class ContainerContextObject : ReniObject, IContextReference
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
            return Container
                .AccessType(_parent, accessPosition, position);
        }

        internal Size StructureSize(int position)
        {
            if(StructureIsDataLess(position))
                return Size.Zero;
            return StructureSize(0, position);
        }

        internal bool StructureIsDataLess(int accessPosition)
        {
            return Container
                .ObtainIsDataLess(Parent, accessPosition);
        }

        internal Result ContextReferenceViaStructReference(int position, Result result)
        {
            return result
                .ReplaceAbsolute(this, () => ContextReferenceViaStructReferenceCode(position), CodeArgs.Arg);
        }

        internal Result Result(Category category, Result innerResult)
        {
            var result = innerResult.ReplaceRelative(this, () => CodeBase.TopRef(RefAlignParam), CodeArgs.Void);
            if(category.HasType)
                result.Type = ToStructure.Type;
            return result;
        }

        internal Size ContextReferenceOffsetFromAccessPoint(int position) { return StructureSize(0, position); }

        Size StructureSize(int fromPosition, int fromNotPosition)
        {
            return Container
                .StructureSize(Parent, fromPosition, fromNotPosition);
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
            return Container
                .StructureSize(Parent, fieldPosition + 1, accessPosition);
        }
    }
}