// 
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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Struct;

namespace Reni.Type
{
    sealed class AccessType : TypeBase
    {
        readonly Structure _accessPoint;
        readonly int _position;
        readonly SimpleCache<AccessManager.IAccessObject> _accessObjectCache;
        readonly SimpleCache<CodeBase> _valueReferenceViaFieldReferenceCodeCache;

        public AccessType(Structure accessPoint, int position)
        {
            Tracer.Assert(!(accessPoint.ValueType(position) is Aligner));
            _accessPoint = accessPoint;
            _position = position;
            _accessObjectCache = new SimpleCache<AccessManager.IAccessObject>(() => _accessPoint.ContainerContextObject.UniqueAccessObject(_position));
            _valueReferenceViaFieldReferenceCodeCache = new SimpleCache<CodeBase>(ObtainValueReferenceViaFieldReferenceCode);
        }

        [EnableDump]
        internal Structure AccessPoint { get { return _accessPoint; } }
        [EnableDump]
        internal int Position { get { return _position; } }
        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return AccessPoint.RefAlignParam; } }
        [DisableDump]
        internal TypeBase ValueType { get { return _accessPoint.ValueType(_position); } }
        [EnableDump]
        AccessManager.IAccessObject AccessObject { get { return _accessObjectCache.Value; } }
        [DisableDump]
        internal override IFunctionalFeature FunctionalFeature { get { return ValueType.FunctionalFeature; } }
        [DisableDump]
        internal override Structure FindRecentStructure { get { return _accessPoint; } }
        [DisableDump]
        TypeBase ValueTypeReference { get { return ValueType.SmartReference(RefAlignParam); } }
        [DisableDump]
        internal override bool IsDataLess { get { return AccessObject.IsDataLess(this); } }

        internal override void Search(SearchVisitor searchVisitor)
        {
            ValueType.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        internal override bool? IsDataLessStructureElement(bool isQuick) { return AccessObject.IsDataLessStructureElement(this, isQuick); }

        internal Result DumpPrintOperationResult(Category category) { return AccessObject.DumpPrintOperationResult(this, category); }
        internal Result DumpPrintFieldResult(Category category) { return GenericDumpPrintResult(category, RefAlignParam); }
        internal Result DumpPrintProcedureCallResult(Category category) { return Void.Result(category); }
        internal Result DumpPrintFunctionResult(Category category) { return Void.Result(category, () => CodeBase.DumpPrintText(ValueType.DumpPrintText), CodeArgs.Void); }

        Result ValueReferenceViaFieldReference(Category category)
        {
            var result = AccessObject.ValueReferenceViaFieldReference(category, this);
            //result.AssertVoidOrValidReference();
            return result;
        }

        internal Result ValueReferenceViaFieldReferenceField(Category category)
        {
            return ValueTypeReference
                .Result(category, ValueReferenceViaFieldReferenceCode, CodeArgs.Arg);
        }

        internal Result ValueReferenceViaFieldReferenceFunction(Category category)
        {
            var trace = ObjectId == 4 && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var argResult = ArgResult(category.Typed);
                Dump("argResult", argResult);
                BreakExecution();
                var valueType = ValueType;
                Dump("valueType", valueType);
                BreakExecution();
                var result = valueType.Result(category, argResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result FieldReferenceViaStructReference(Category category)
        {
            if(AccessObject.IsDataLess(this))
                return Result(category);
            return Result(category, () => AccessPoint.ReferenceType.ArgCode(), CodeArgs.Arg);
        }

        CodeBase ValueReferenceViaFieldReferenceCode()
        {
            if(!_valueReferenceViaFieldReferenceCodeCache.IsBusy)
                return _valueReferenceViaFieldReferenceCodeCache.Value;
            NotImplementedMethod();
            return null;
        }

        CodeBase ObtainValueReferenceViaFieldReferenceCode()
        {
            return ArgCode()
                .AddToReference(RefAlignParam, AccessPoint.FieldOffset(Position));
        }

        internal override TypeBase AutomaticDereference() { return ValueType; }

        Result DereferenceResult(Category category)
        {
            return ValueReferenceViaFieldReference(category)
                .AutomaticDereference();
        }
        Result ToAutomaticReferenceResult(Category category)
        {
            var result = ValueReferenceViaFieldReference(category);
            //result.AssertEmptyOrValidReference();
            return result;
        }
    }
}