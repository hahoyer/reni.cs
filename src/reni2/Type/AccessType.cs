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
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class AccessType : TypeBase
    {
        private static int _nextObjectId;
        private readonly TypeBase _valueType;
        private readonly Structure _accessPoint;
        private readonly int _position;
        private readonly AssignmentFeature _assignmentFeature;
        private readonly SimpleCache<AccessManager.IAccessObject> _accessObjectCache;

        public AccessType(TypeBase valueType, Structure accessPoint, int position)
            : base(_nextObjectId++)
        {
            _valueType = valueType;
            _accessPoint = accessPoint;
            _position = position;
            _assignmentFeature = new AssignmentFeature(this);
            _accessObjectCache = new SimpleCache<AccessManager.IAccessObject>(SpawnAccessObject);
        }

        private AccessManager.IAccessObject SpawnAccessObject() { return _accessPoint.ContainerContextObject.SpawnAccessObject(_position); }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return _accessPoint.RefAlignParam; } }

        [EnableDump]
        internal Structure AccessPoint { get { return _accessPoint; } }

        [EnableDump]
        internal TypeBase ValueType { get { return _valueType; } }

        [EnableDump]
        internal int Position { get { return _position; } }

        [DisableDump]
        private AccessManager.IAccessObject AccessObject { get { return _accessObjectCache.Value; } }

        internal Result DumpPrintOperationResult(Category category) { return AccessObject.DumpPrintOperationResult(this, category); }
        internal Result DumpPrintFieldResult(Category category) { return OperationResult<IFeature>(category, DumpPrintToken.Create(), RefAlignParam); }
        internal Result DumpPrintProcedureCallResult(Category category) { return Void.Result(category); }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _valueType.Search(searchVisitor.Child(this));
            _valueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override TypeBase ToReference(RefAlignParam refAlignParam) { return this; }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            return ValueType
                .ToReference(RefAlignParam)
                .Conversion(category, dest)
                .ReplaceArg(ValueReferenceViaFieldReference(category));
        }

        internal Result ApplyAssignment(Category category)
        {
            var result = new Result
                (category
                 , () => RefAlignParam.RefSize
                 , () => SpawnFunctionalType(_assignmentFeature)
                 , ArgCode
                 , Refs.None
                );
            return result;
        }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            return new Result
                (category
                 , () => Size.Zero
                 , () => Void
                 , () => AssignmentCode(argsType)
                );
        }

        private CodeBase AssignmentCode(TypeBase argsType)
        {
            var sourceResult = argsType
                .ConvertTo(Category.Code | Category.Type, ValueType).Code;
            return CodeBase
                .Arg(argsType.SpawnReference(RefAlignParam))
                .Sequence(sourceResult)
                .Assignment(RefAlignParam, ValueType.Size);
        }

        private Result ValueReferenceViaFieldReference(Category category)
        {
            var result = new Result
                (category
                 , () => RefAlignParam.RefSize
                 , () => ValueType.ToReference(RefAlignParam)
                 , ValueReferenceViaFieldReferenceCode
                 , Refs.None
                );
            return result;
        }

        private CodeBase ValueReferenceViaFieldReferenceCode()
        {
            return CodeBase
                .Arg(this)
                .AddToReference(RefAlignParam, AccessPoint.FieldOffsetFromThisReference(Position));
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return ValueType.IsConvertableTo(dest, conversionParameter); }

        protected override Size GetSize() { return _accessPoint.RefAlignParam.RefSize; }
        internal override bool IsRef(RefAlignParam refAlignParam) { return refAlignParam == RefAlignParam; }

        internal override TypeBase AutomaticDereference()
        {
            NotImplementedMethod();
            return null;
            ;
        }

        protected override TypeBase Dereference()
        {
            NotImplementedMethod();
            return null;
            ;
        }

        protected override Result DereferenceResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
            ;
        }
    }
}