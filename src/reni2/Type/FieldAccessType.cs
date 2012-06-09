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
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type
{
    sealed class FieldAccessType : Child<TypeBase>, ISetterTargetType, ISoftReference
    {
        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly int _position;
        [DisableDump]
        internal readonly ISuffixFeature AssignmentFeature;

        internal FieldAccessType(Structure structure, int position)
            : base(structure.ValueType(position))
        {
            AssignmentFeature = new AssignmentFeature(this);
            _structure = structure;
            _position = position;
        }

        TypeBase ISetterTargetType.ValueType { get { return Parent; } }
        TypeBase ISetterTargetType.Type { get { return this; } }

        Size IContextReference.Size { get { return RefAlignParam.RefSize; } }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        RefAlignParam RefAlignParam { get { return _structure.RefAlignParam; } }
        [DisableDump]
        IContextReference ObjectReference { get { return this; } }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return Parent.TypeForTypeOperator; } }

        Result IReference.DereferenceResult(Category category) { return ParentConversionResult(category); }

        public override string NodeDump { get { return base.NodeDump + "{" + _structure.NodeDump + "@" + _position + "}"; } }

        protected override Size GetSize() { return RefAlignParam.RefSize; }

        internal override Result SmartLocalReferenceResult(Category category)
        {
            return UniqueAlign(Root.DefaultRefAlignParam.AlignBits)
                .Result
                (category
                 , () => LocalReferenceCode().Dereference(Root.DefaultRefAlignParam.RefSize)
                 , () => Destructor(Category.CodeArgs).CodeArgs + CodeArgs.Arg()
                );
        }

        protected override Result ParentConversionResult(Category category)
        {
            return Parent.SmartReference()
                .Result(category, ArgResult(category.Typed).AddToReference(() => _structure.FieldOffset(_position)));
        }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => Parent);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
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
            return Pair(Parent.SmartReference()).ArgCode
                .Assignment(RefAlignParam, Parent.Size);
        }

        TypeBase IReference.TargetType { get { return Parent; } }
    }
}