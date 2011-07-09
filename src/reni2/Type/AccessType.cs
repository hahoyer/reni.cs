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
    internal sealed class AccessType : ReferenceType
    {
        private readonly Structure _accessPoint;
        private readonly int _position;
        private readonly SimpleCache<AssignmentFeature> _assignmentFeatureCache;
        private readonly SimpleCache<AccessManager.IAccessObject> _accessObjectCache;

        public AccessType(TypeBase valueType, Structure accessPoint, int position)
            : base(valueType)
        {
            Tracer.Assert(!(valueType is Aligner));
            _accessPoint = accessPoint;
            _position = position;
            _assignmentFeatureCache = new SimpleCache<AssignmentFeature>(() => new AssignmentFeature(this));
            _accessObjectCache = new SimpleCache<AccessManager.IAccessObject>(() => _accessPoint.ContainerContextObject.UniqueAccessObject(_position));
        }

        [EnableDump]
        internal Structure AccessPoint { get { return _accessPoint; } }
        [EnableDump]
        internal int Position { get { return _position; } }
        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return AccessPoint.RefAlignParam; } }
        [DisableDump]
        private AccessManager.IAccessObject AccessObject { get { return _accessObjectCache.Value; } }
        [DisableDump]
        internal override IFunctionalFeature FunctionalFeature { get { return AccessValueType.FunctionalFeature; } }
        [DisableDump]
        internal override Structure FindRecentStructure { get { return _accessPoint; } }
        [DisableDump]
        private TypeBase AccessValueType { get { return AccessObject.ValueType(this); } }
        [DisableDump]
        internal TypeBase ValueTypeProperty { get { return ValueType.PropertyResult(Category.Type).Type; } }
        [DisableDump]
        internal TypeBase ValueTypeField { get { return ValueType; } }
        [DisableDump]
        internal TypeBase ValueTypeFunction { get { return ValueType; } }
        [DisableDump]
        private TypeBase ValueTypeReference { get { return AccessValueType.ToReference(RefAlignParam); } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            AccessValueType.Search(searchVisitor.Child(this));
            AccessValueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal Result AssignmentFeatureResult(Category category, RefAlignParam refAlignParam)
        {
            var result = new Result
                (category
                 , () => refAlignParam.RefSize
                 , () => UniqueFunctionalType(_assignmentFeatureCache.Value, refAlignParam)
                 , ArgCode
                 , Refs.None
                );
            return result;
        }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            var typedCategory = category | Category.Type;
            var sourceResult = argsType.Conversion(typedCategory, ValueTypeReference);
            var destinationResult = ValueReferenceViaFieldReference(typedCategory)
                .ReplaceArg(FieldReferenceViaStructReference(typedCategory))
                .ReplaceArg(AccessPoint.StructReferenceViaContextReference(typedCategory));
            var resultForArg = destinationResult.Pair(sourceResult);
            return AssignmentResult(category).ReplaceArg(resultForArg);
        }

        private Result AssignmentResult(Category category)
        {
            return new Result
                (category
                 , () => Size.Zero
                 , () => Void
                 , AssignmentCode
                );
        }

        private CodeBase AssignmentCode()
        {
            return ValueTypeReference
                .Pair(ValueTypeReference)
                .ArgCode()
                .Assignment(RefAlignParam, AccessValueType.Size);
        }

        internal Result DumpPrintOperationResult(Category category) { return AccessObject.DumpPrintOperationResult(this, category); }
        internal Result DumpPrintFieldResult(Category category) { return OperationResult<IFeature>(category, DumpPrintToken.Create(), RefAlignParam); }
        internal Result DumpPrintProcedureCallResult(Category category) { return Void.Result(category); }
        internal Result DumpPrintFunctionResult(Category category) { return Void.Result(category, () => CodeBase.DumpPrintText(ValueType.DumpPrintText)); }

        private Result ValueReferenceViaFieldReference(Category category) { return AccessObject.ValueReferenceViaFieldReference(category, this); }
        internal Result ValueReferenceViaFieldReferenceProperty(Category category)
        {
            StartMethodDump(ObjectId == -3, category);
            try
            {
                BreakExecution();
                var result = ValueType
                    .PropertyResult(category)
                    .LocalReferenceResult(RefAlignParam)
                    .ContextReferenceViaStructReference(AccessPoint)
                    .ReplaceArg(()=>StructReferenceViaFieldReference(category));
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        private Result StructReferenceViaFieldReference(Category category) { return AccessPoint.ReferenceType.Result(category, ArgCode); }
        internal Result ValueReferenceViaFieldReferenceField(Category category) { return ValueTypeReference.Result(category, ValueReferenceViaFieldReferenceCode); }

        internal Result FieldReferenceViaStructReference(Category category) { return Result(category, () => AccessPoint.ReferenceType.ArgCode()); }

        private CodeBase ValueReferenceViaFieldReferenceCode() { return ArgCode().AddToReference(RefAlignParam, AccessPoint.FieldOffset(Position)); }

        internal override TypeBase AutomaticDereference() { return AccessValueType; }
        protected override CodeBase DereferenceCode() { return ValueReferenceViaFieldReferenceCode().Dereference(RefAlignParam, AccessValueType.Size); }

        internal override bool VirtualIsConvertable(AutomaticReferenceType destination, ConversionParameter conversionParameter)
        {
            return
                AccessValueType == destination.ValueType
                || AccessValueType.IsConvertable(destination.ValueType, conversionParameter);
        }

        internal override Result VirtualForceConversion(Category category, AutomaticReferenceType destination)
        {
            StartMethodDump(ObjectId == -3, category, destination);
            try
            {
                BreakExecution();
                var fieldAsValue = ValueReferenceViaFieldReference(category.Typed);
                Dump("fieldAsValue", fieldAsValue);
                var result = fieldAsValue.Conversion(destination);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}