#region Copyright (C) 2012

// 
//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Struct;

namespace Reni.Type
{
    sealed class FieldAccessType : Child<TypeBase>, ISetterTargetType, IReferenceInCode, IReference
    {
        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly int _position;
        readonly DictionaryEx<RefAlignParam, TypeBase> _setterTypeCache;

        internal FieldAccessType(Structure structure, int position)
            : base(structure.ValueType(position))
        {
            _structure = structure;
            _position = position;
            _setterTypeCache = new DictionaryEx<RefAlignParam, TypeBase>(rap => new SetterType(this, rap));
        }
        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        TypeBase ISetterTargetType.ValueType { get { return Parent; } }
        [DisableDump]
        IReferenceInCode ISetterTargetType.ObjectReference { get { return ObjectReference; } }
        [DisableDump]
        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        [DisableDump]
        RefAlignParam IReference.RefAlignParam { get { return RefAlignParam; } }
        [DisableDump]
        RefAlignParam RefAlignParam { get { return _structure.RefAlignParam; } }
        [DisableDump]
        IReferenceInCode ObjectReference { get { return this; } }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return Parent.TypeForTypeOperator; } }

        Result IReference.DereferenceResult(Category category) { return ParentConversionResult(category); }

        public override string NodeDump { get { return base.NodeDump + "{" + _structure.NodeDump + "@" + _position + "}"; } }

        protected override Size GetSize() { return RefAlignParam.RefSize; }

        internal override Result SmartLocalReferenceResult(Category category, RefAlignParam refAlignParam)
        {
            return UniqueAlign(refAlignParam.AlignBits)
                .Result
                (category
                 , () => LocalReferenceCode(refAlignParam).Dereference(refAlignParam, refAlignParam.RefSize)
                 , () => Destructor(Category.CodeArgs).CodeArgs + CodeArgs.Arg()
                );
        }

        protected override Result ParentConversionResult(Category category)
        {
            return Parent.SmartReference(RefAlignParam)
                .Result
                (category
                 , () => ArgCode().AddToReference(RefAlignParam, _structure.FieldOffset(_position))
                 , CodeArgs.Arg
                );
        }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        public Result AssignmentFeatureResult(Category category, RefAlignParam refAlignParam)
        {
            var result = new Result
                (category
                 , () => false
                 , () => refAlignParam.RefSize
                 , () => _setterTypeCache.Find(refAlignParam)
                 , ArgCode
                 , CodeArgs.Arg
                );
            return result;
        }

        internal override int SequenceCount(TypeBase elementType) { return Parent.SequenceCount(elementType); }

        Result ISetterTargetType.Result(Category category, TypeBase valueType)
        {
            var typedCategory = category.Typed;
            var sourceResult = valueType
                .UniqueReference(RefAlignParam)
                .Type
                .Conversion(typedCategory, Parent.UniqueReference(RefAlignParam).Type);
            var destinationResult = DestinationResult(typedCategory);
            var resultForArg = destinationResult + sourceResult;
            return AssignmentResult(category).ReplaceArg(resultForArg);
        }

        Result DestinationResult(Category typedCategory)
        {
            return Result
                (typedCategory
                 , ObjectReference
                 , () => _structure.FieldOffset(_position)
                );
        }

        Result AssignmentResult(Category category)
        {
            return new Result
                (category
                 , () => true
                 , () => Size.Zero
                 , () => Void
                 , AssignmentCode
                 , CodeArgs.Arg
                );
        }

        CodeBase AssignmentCode()
        {
            return Pair(Parent.SmartReference(RefAlignParam))
                .ArgCode()
                .Assignment(RefAlignParam, Parent.Size);
        }

        protected override IConverter ConverterForDifferentTypes(ConversionParameter conversionParameter, TypeBase destination)
        {
            return new FunctionalConverter(ParentConversionResult)
                .Concat(Parent.SmartReference(RefAlignParam).Converter(conversionParameter, destination));
        }

        TypeBase IReference.Type { get { return this; } }
        TypeBase IReference.TargetType { get { return Parent; } }
    }
}