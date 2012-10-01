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
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Bit array operation
    /// </summary>
    [Serializable]
    sealed class BitArrayBinaryOp : BinaryOp
    {
        [Node]
        [DisableDump]
        internal readonly string OpToken;

        readonly Size _size;

        internal BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
            : base(leftSize, rightSize)
        {
            OpToken = opToken;
            _size = size;
            StopByObjectId(-381);
        }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        [DisableDump]
        internal override Size OutputSize { get { return _size; } }

        internal override void Visit(IVisitor visitor) { visitor.BitArrayBinaryOp(OpToken, OutputSize, LeftSize, RightSize); }

        protected override string GetNodeDump() { return base.GetNodeDump() + " <" + LeftSize + "> " + OpToken + " <" + RightSize + ">"; }
    }

    /// <summary>
    ///     Bit array prefix operation
    /// </summary>
    [Serializable]
    sealed class BitArrayPrefixOp : FiberItem
    {
        [Node]
        [DisableDump]
        internal readonly ISequenceOfBitPrefixOperation OpToken;

        readonly Size _size;

        [Node]
        [DisableDump]
        internal readonly Size ArgSize;

        internal BitArrayPrefixOp(ISequenceOfBitPrefixOperation name, Size size, Size argSize)
        {
            OpToken = name;
            _size = size;
            ArgSize = argSize;
        }

        [DisableDump]
        internal override Size InputSize { get { return ArgSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _size; } }

        internal override void Visit(IVisitor visitor) { visitor.BitArrayPrefixOp(OpToken, OutputSize, ArgSize); }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        protected override string GetNodeDump() { return base.GetNodeDump() + " " + OpToken.DataFunctionName + " " + ArgSize; }
    }

    /// <summary>
    ///     Dump and print
    /// </summary>
    [Serializable]
    sealed class DumpPrintNumberOperation : BinaryOp
    {
        internal DumpPrintNumberOperation(Size leftSize, Size rightSize)
            : base(leftSize, rightSize) { }

        [DisableDump]
        internal override Size OutputSize { get { return Size.Zero; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + " <" + LeftSize + "> dump_print <" + RightSize + ">"; }

        internal override void Visit(IVisitor visitor) { visitor.PrintNumber(LeftSize, RightSize); }
    }

    [Serializable]
    sealed class DumpPrintTextOperation : FiberItem
    {
        readonly Size _leftSize;
        readonly Size _itemSize;
        internal DumpPrintTextOperation(Size leftSize, Size itemSize)
        {
            _leftSize = leftSize;
            _itemSize = itemSize;
        }

        internal override Size InputSize { get { return _leftSize; } }
        [DisableDump]
        internal override Size OutputSize { get { return Size.Zero; } }
        protected override string GetNodeDump() { return base.GetNodeDump() + " <" + InputSize + "> dump_print_text(" + _itemSize + ")"; }
        internal override void Visit(IVisitor visitor) { visitor.PrintText(InputSize, _itemSize); }
    }

    [Serializable]
    sealed class DumpPrintText : FiberHead
    {
        [Node]
        [EnableDump]
        readonly string _dumpPrintText;

        internal DumpPrintText(string dumpPrintText) { _dumpPrintText = dumpPrintText; }

        protected override Size GetSize() { return Size.Zero; }
        internal override void Visit(IVisitor visitor) { visitor.PrintText(_dumpPrintText); }

        protected override string GetNodeDump() { return base.GetNodeDump() + " dump_print " + _dumpPrintText.Quote(); }
    }
}