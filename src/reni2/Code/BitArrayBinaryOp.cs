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
    internal sealed class BitArrayBinaryOp : BinaryOp
    {
        [Node]
        [DisableDump]
        internal readonly ISequenceOfBitBinaryOperation OpToken;

        private readonly Size _size;

        internal BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
            : base(leftSize, rightSize)
        {
            OpToken = opToken;
            _size = size;
            StopByObjectId(-381);
        }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        [DisableDump]
        internal override Size OutputSize { get { return _size; } }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.BitArrayBinaryOp(OpToken, OutputSize, LeftSize); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitArrayBinaryOp(OpToken, OutputSize, LeftSize, RightSize); }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " <" + LeftSize + "> " + OpToken.DataFunctionName + " <" + RightSize + ">"; } }
    }

    /// <summary>
    ///     Bit array prefix operation
    /// </summary>
    [Serializable]
    internal sealed class BitArrayPrefixOp : FiberItem
    {
        [Node]
        [DisableDump]
        internal readonly ISequenceOfBitPrefixOperation OpToken;

        private readonly Size _size;

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

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitArrayPrefixOp(OpToken, OutputSize, ArgSize); }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.BitArrayPrefix(OpToken, OutputSize); }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " " + OpToken.DataFunctionName + " " + ArgSize; } }
    }

    /// <summary>
    ///     Dump and print
    /// </summary>
    [Serializable]
    internal sealed class DumpPrintOperation : BinaryOp
    {
        internal DumpPrintOperation(Size leftSize, Size rightSize)
            : base(leftSize, rightSize) { }

        [DisableDump]
        internal override Size OutputSize { get { return Size.Zero; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " <" + LeftSize + "> dump_print <" + RightSize + ">"; } }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.DumpPrint(top, InputSize); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.DumpPrintOperation(LeftSize, RightSize); }
    }

    [Serializable]
    internal sealed class DumpPrintText : FiberHead
    {
        [Node]
        [EnableDump]
        private readonly string _dumpPrintText;

        internal DumpPrintText(string dumpPrintText) { _dumpPrintText = dumpPrintText; }

        protected override Size GetSize() { return Size.Zero; }
        protected override string CSharpString() { return CSharpGenerator.DumpPrintText(_dumpPrintText); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.DumpPrintText(_dumpPrintText); }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " dump_print " + _dumpPrintText.Quote(); } }
    }
}