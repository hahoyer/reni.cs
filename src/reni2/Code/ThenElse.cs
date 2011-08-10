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
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Then-Else construct
    /// </summary>
    internal sealed class ThenElse : FiberItem
    {
        private static int _nextId;

        [Node]
        private readonly Size _condSize;

        [Node]
        internal readonly CodeBase ThenCode;

        [Node]
        internal readonly CodeBase ElseCode;

        internal ThenElse(CodeBase thenCode, CodeBase elseCode)
            : this(Size.Create(1), thenCode, elseCode) { }

        private ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
            : base(_nextId++)
        {
            _condSize = condSize;
            ThenCode = thenCode;
            ElseCode = elseCode;
        }

        private Size MaxSizeImplementation
        {
            get
            {
                var tSize = ThenCode.MaxSize;
                var eSize = ElseCode.MaxSize;
                return tSize.Max(eSize);
            }
        }

        protected override Refs GetRefsImplementation() { return ThenCode.Refs.Sequence(ElseCode.Refs); }

        internal override FiberItem[] TryToCombineBack(BitCast preceding)
        {
            if(preceding.InputSize == preceding.OutputSize)
                return null;
            return new FiberItem[]
                   {
                       new BitCast(preceding.InputSize, preceding.InputSize, Size.Create(1)),
                       new ThenElse(preceding.InputSize, ThenCode, ElseCode)
                   };
        }

        internal override Size InputSize { get { return _condSize; } }
        internal override Size OutputSize { get { return ThenCode.Size; } }
        protected override FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ThenElse(this); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.ThenElse(_condSize, ThenCode, ElseCode); }
        internal FiberItem ReCreate(CodeBase newThen, CodeBase newElse) { return new ThenElse(_condSize, newThen ?? ThenCode, newElse ?? ElseCode); }
    }

    [Serializable]
    internal sealed class Then : FiberItem
    {
        [Node]
        internal readonly Size CondSize;

        internal Then(Size condSize) { CondSize = condSize; }

        internal override Size OutputSize { get { return Size.Zero; } }
        internal override Size InputSize { get { return CondSize; } }

        internal override FiberItem[] TryToCombineBack(BitCast preceding)
        {
            if(preceding.OutputSize == CondSize)
                return new[] {new Then(preceding.InputSize)};
            return null;
        }

        internal override FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement)
        {
            if(precedingElement.OutputSize == CondSize)
                return new[] {new BitArrayOpThen(this, precedingElement)};
            return null;
        }

        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }

    [Serializable]
    internal sealed class BitArrayOpThen : FiberItem
    {
        [Node]
        [EnableDump]
        private readonly BitArrayBinaryOp _bitArrayBinaryOp;

        [Node]
        [EnableDump]
        private readonly Then _thenCode;

        public BitArrayOpThen(Then thenCode, BitArrayBinaryOp bitArrayBinaryOp)
        {
            _thenCode = thenCode;
            _bitArrayBinaryOp = bitArrayBinaryOp;
        }

        internal override Size InputSize { get { return _bitArrayBinaryOp.DeltaSize + _bitArrayBinaryOp.OutputSize; } }
        internal override Size OutputSize { get { return _thenCode.OutputSize; } }
        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }
}