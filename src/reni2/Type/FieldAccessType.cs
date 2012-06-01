#region Copyright (C) 2012

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
        readonly SimpleCache<SetterType> _setterTypeCache;

        internal FieldAccessType(Structure structure, int position)
            : base(structure.ValueType(position))
        {
            _structure = structure;
            _position = position;
            _setterTypeCache = new SimpleCache<SetterType>(() => new SetterType(this));
        }

        TypeBase ISetterTargetType.ValueType { get { return Parent; } }
        IReferenceInCode ISetterTargetType.ObjectReference { get { return ObjectReference; } }
        TypeBase ISetterTargetType.Type { get { return this; } }
        SetterType ISetterTargetType.SetterType { get { return _setterTypeCache.Value; } }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        Size IReferenceInCode.RefSize { get { return RefAlignParam.RefSize; } }
        RefAlignParam IReference.RefAlignParam { get { return RefAlignParam; } }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
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
                 , () => LocalReferenceCode(refAlignParam).Dereference(refAlignParam.RefSize)
                 , () => Destructor(Category.CodeArgs).CodeArgs + CodeArgs.Arg()
                );
        }

        protected override Result ParentConversionResult(Category category)
        {
            return Parent.SmartReference(RefAlignParam)
                .Result(category, ArgResult(category.Typed).AddToReference(()=>_structure.FieldOffset(_position)));
        }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, ()=>Parent);
        }

        internal override int SequenceCount(TypeBase elementType) { return Parent.SequenceCount(elementType); }

        Result ISetterTargetType.Result(Category category)
        {
            return new Result
                (category
                 , getType: () => Void
                 , getCode: AssignmentCode
                 , getArgs: CodeArgs.Arg
                );
        }

        Result ISetterTargetType.DestinationResult(Category category)
        {
            return Result(category, ObjectReference)
                .AddToReference(() => _structure.FieldOffset(_position));
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