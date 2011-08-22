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
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class AccessType : ReferenceType
    {
        private readonly Structure _accessPoint;
        private readonly int _position;
        private readonly SimpleCache<AssignmentFeature> _assignmentFeatureCache;
        private readonly SimpleCache<AccessManager.IAccessObject> _accessObjectCache;

        public AccessType(Structure accessPoint, int position)
            : base(accessPoint.ValueType(position))
        {
            Tracer.Assert(!(accessPoint.ValueType(position) is Aligner));
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
        internal override IFunctionalFeature FunctionalFeature { get { return ValueType.FunctionalFeature; } }
        [DisableDump]
        internal override Structure FindRecentStructure { get { return _accessPoint; } }
        [DisableDump]
        internal override TypeBase ValueType { get { return AccessObject.ValueType(this); } }
        [DisableDump]
        internal TypeBase ValueTypeProperty { get { return base.ValueType.PropertyResult(Category.Type).Type; } }
        [DisableDump]
        internal TypeBase ValueTypeField { get { return base.ValueType; } }
        [DisableDump]
        internal TypeBase ValueTypeFunction { get { return base.ValueType; } }
        [DisableDump]
        private TypeBase ValueTypeReference { get { return ValueType.ForceReference(RefAlignParam); } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            ValueType.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        internal Result AssignmentFeatureResult(Category category, RefAlignParam refAlignParam)
        {
            var result = new Result
                (category
                 , () => refAlignParam.RefSize
                 , () => _assignmentFeatureCache.Value.UniqueFunctionalType(refAlignParam)
                 , ArgCode
                 , Refs.Arg
                );
            return result;
        }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            var typedCategory = category.Typed;
            var sourceResult = argsType.Conversion(typedCategory, ValueTypeReference);
            var destinationResult = ValueReferenceViaFieldReference(typedCategory)
                .ReplaceArg(FieldReferenceViaStructReference(typedCategory))
                .ReplaceArg(AccessPoint.StructReferenceViaContextReference(typedCategory));
            var resultForArg = destinationResult.Sequence(sourceResult);
            return AssignmentResult(category).ReplaceArg(resultForArg);
        }

        private Result AssignmentResult(Category category)
        {
            return new Result
                (category
                 , () => Size.Zero
                 , () => Void
                 , AssignmentCode
                 , Refs.Arg
                );
        }

        private CodeBase AssignmentCode()
        {
            return ValueTypeReference
                .Pair(ValueTypeReference)
                .ArgCode()
                .Assignment(RefAlignParam, ValueType.Size);
        }

        internal Result DumpPrintOperationResult(Category category) { return AccessObject.DumpPrintOperationResult(this, category); }
        internal Result DumpPrintFieldResult(Category category) { return GenericDumpPrintResult(category, RefAlignParam); }
        internal Result DumpPrintProcedureCallResult(Category category) { return Void.Result(category); }
        internal Result DumpPrintFunctionResult(Category category) { return Void.Result(category, () => CodeBase.DumpPrintText(base.ValueType.DumpPrintText), Refs.Void); }

        internal Result ValueReferenceViaFieldReference(Category category) { return AccessObject.ValueReferenceViaFieldReference(category, this); }
        internal Result ValueReferenceViaFieldReferenceProperty(Category category)
        {
            StartMethodDump(ObjectId == 13 && category.HasCode, category);
            try
            {
                BreakExecution();
                var propertyResult = base.ValueType.PropertyResult(category);
                Dump("propertyResult", propertyResult);
                var localReferenceResult = propertyResult.LocalReferenceResult(RefAlignParam);
                Dump("localReferenceResult", localReferenceResult);
                var replaceObjectRefByArg = localReferenceResult
                    .ContextReferenceViaStructReference(AccessPoint);
                Dump("replaceObjectRefByArg", replaceObjectRefByArg);
                var structReferenceViaFieldReference = StructReferenceViaFieldReference(category.Typed);
                Dump("structReferenceViaFieldReference", structReferenceViaFieldReference);
                BreakExecution();
                var result = replaceObjectRefByArg
                    .ReplaceArg(structReferenceViaFieldReference);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        private Result StructReferenceViaFieldReference(Category category)
        {
            return AccessPoint
                .ReferenceType
                .Result(category, ArgResult(category));
        }

        internal Result ValueReferenceViaFieldReferenceField(Category category) { return ValueTypeReference.Result(category, ValueReferenceViaFieldReferenceCode, Refs.Arg); }

        internal Result FieldReferenceViaStructReference(Category category) { return Result(category, () => AccessPoint.ReferenceType.ArgCode(), Refs.Arg); }

        private CodeBase ValueReferenceViaFieldReferenceCode() { return ArgCode().AddToReference(RefAlignParam, AccessPoint.FieldOffset(Position)); }

        internal override TypeBase AutomaticDereference() { return ValueType; }
        internal override Result DereferenceResult(Category category) { return ValueReferenceViaFieldReference(category).AutomaticDereference(); }
        internal override Result ToAutomaticReferenceResult(Category category) { return ValueReferenceViaFieldReference(category); }
    }
}