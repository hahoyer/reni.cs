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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Sequence;

namespace Reni.Type
{
    [Serializable]
    sealed class BitType : TypeBase
    {
        readonly Root _rootContext;

        internal BitType(Root rootContext) { _rootContext = rootContext; }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "bit"; } }
        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }

        protected override Size GetSize() { return Size.Create(1); }

        internal Result DumpPrintResult(Category category, SequenceType sequenceType, ArrayType arrayType)
        {
            Tracer.Assert(sequenceType.Parent == arrayType);
            Tracer.Assert(arrayType.ElementType == this);
            return VoidType
                .Result(category, sequenceType.DumpPrintNumberCode, CodeArgs.Arg);
        }

        internal Result DumpPrintResult(Category category)
        {
            return VoidType
                .Result(category, DumpPrintNumberCode, CodeArgs.Arg);
        }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, null);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        protected override string Dump(bool isRecursion) { return GetType().PrettyName(); }

        internal override string DumpShort() { return "bit"; }
        internal SequenceType UniqueNumber(int bitCount) { return UniqueArray(bitCount).UniqueSequence; }
        internal Result Result(Category category, BitsConst bitsConst)
        {
            return UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, getCode: () => CodeBase.BitsConst(bitsConst));
        }
        internal CodeBase Apply(Size size, string token, int objectBits, int argsBits)
        {
            var objectType = UniqueNumber(objectBits).UniqueAlign;
            var argsType = UniqueNumber(argsBits).UniqueAlign;
            return objectType
                .Pair(argsType).ArgCode
                .BitSequenceOperation(token, size, Size.Create(objectBits).ByteAlignedSize);
        }
    }
}