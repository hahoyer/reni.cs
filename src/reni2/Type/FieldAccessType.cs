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
using Reni.ReniParser;
using Reni.Struct;

namespace Reni.Type
{
    sealed class FieldAccessType : SetterTargetType
    {
        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly int _position;

        internal FieldAccessType(Structure structure, int position)
        {
            _structure = structure;
            _position = position;
        }

        internal override TypeBase ValueType { get { return _structure.ValueType(_position); } }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        RefAlignParam RefAlignParam { get { return _structure.RefAlignParam; } }
        [DisableDump]
        IContextReference ObjectReference { get { return this; } }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return ValueType.TypeForTypeOperator; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + "{" + _structure.NodeDump + "@" + _position + "}"; }

        protected override Size GetSize() { return RefAlignParam.RefSize; }

        Size GetFieldOffset() { return _structure.FieldOffset(_position); }

        internal override Result GetterResult(Category category)
        {
            return ValueType.SmartPointer
                .Result(category, ArgResult(category.Typed).AddToReference(GetFieldOffset));
        }

        internal override Result SetterResult(Category category)
        {
            return new Result
                (category
                 , getCode: () => Pair(ValueType.SmartPointer).ArgCode.Assignment(ValueType.Size)
                 , getArgs: CodeArgs.Arg
                );
        }

        internal override Result DestinationResult(Category category)
        {
            return Result(category, ObjectReference)
                .AddToReference(GetFieldOffset);
        }

        internal override void Search(SearchVisitor searchVisitor, ExpressionSyntax syntax)
        {
            searchVisitor.Search(this, () => ValueType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor, syntax);
        }

        internal override int? SmartSequenceLength(TypeBase elementType)
        {
            return ValueType
                .SmartSequenceLength(elementType);
        }

        internal override int? SmartArrayLength(TypeBase elementType)
        {
            return ValueType
                .SmartArrayLength(elementType);
        }
    }
}