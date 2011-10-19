// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    [Serializable]
    sealed class Bit : TypeBase
    {
        internal override bool IsDataLess { get { return false; } }
        protected override Size GetSize() { return Size.Create(1); }

        internal override string DumpPrintText { get { return "bit"; } }
        internal override int SequenceCount(TypeBase elementType) { return 1; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        protected override string Dump(bool isRecursion) { return GetType().PrettyName(); }

        internal override string DumpShort() { return "bit"; }

        internal static CodeBase BitSequenceOperation(Size size, ISequenceOfBitBinaryOperation token, int objectBits, int argsBits)
        {
            var objectType = UniqueNumber(objectBits).UniqueAlign(BitsConst.SegmentAlignBits);
            var argsType = UniqueNumber(argsBits).UniqueAlign(BitsConst.SegmentAlignBits);
            return objectType
                .Pair(argsType)
                .ArgCode()
                .BitSequenceOperation(token, size, Size.Create(objectBits).ByteAlignedSize);
        }
    }
}