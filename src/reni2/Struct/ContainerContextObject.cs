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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
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
        internal RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return TypeBase.UniqueNumber(IndexSize); } }

        [DisableDump]
        internal Root RootContext { get { return Parent.RootContext; } }

        [DisableDump]
        internal Structure ToStructure { get { return _parent.UniqueStructure(Container); } }

        private int IndexSize { get { return Container.IndexSize; } }

        internal AccessManager.IAccessObject UniqueAccessObject(int position) { return _accessObjectsCache.Find(position); }
        internal Structure UniqueAccessPoint(int position) { return Parent.UniqueStructure(Container,position); }
        internal TypeBase InnerType(int accessPosition, int position) { return Container.InnerType(_parent, accessPosition, position); }

        internal Size StructureSize(int position)
        {
            if (StructureIsDataLess(false, position) == true)
                return Size.Zero;
            return StructureSize(0, position);
        }
        internal bool? StructureIsDataLess(bool isQuick, int accessPosition) { return Container.IsDataLess(isQuick, Parent, accessPosition); }
        internal bool IsDataLess(int position) { return Container.StructureResult(Category.IsDataLess, Parent, position, position + 1).SmartIsDataLess; }

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

        Size StructureSize(int fromPosition, int fromNotPosition) { return Container.StructureSize(Parent, fromPosition, fromNotPosition); }

        private bool IsLambda(int position) { return Container.IsLambda(position); }

        private AccessManager.IAccessObject GetAccessObject(int position)
        {
            if(IsLambda(position))
                return AccessManager.Function;
            if(IsDataLess(position))
                return AccessManager.ProcedureCall;
            return AccessManager.Field;
        }

        private CodeBase ContextReferenceViaStructReferenceCode(int accessPosition)
        {
            return Parent
                .UniqueStructure(Container,accessPosition)
                .ReferenceType
                .ArgCode()
                .AddToReference(RefAlignParam, ContextReferenceOffsetFromAccessPoint(accessPosition));
        }

        internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition) { return Container.StructureSize(Parent, fieldPosition + 1, accessPosition); }
    }
}