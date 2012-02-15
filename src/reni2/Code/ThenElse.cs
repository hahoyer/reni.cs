// 
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
    sealed class ThenElse : FiberItem
    {
        static int _nextId;

        [Node]
        readonly Size _condSize;

        [Node]
        internal readonly CodeBase ThenCode;

        [Node]
        internal readonly CodeBase ElseCode;

        internal ThenElse(CodeBase thenCode, CodeBase elseCode)
            : this(Size.Create(1), thenCode, elseCode) { }

        ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
            : base(_nextId++)
        {
            _condSize = condSize;
            ThenCode = thenCode;
            ElseCode = elseCode;
        }

        protected override CodeArgs GetRefsImplementation() { return ThenCode.CodeArgs.Sequence(ElseCode.CodeArgs); }

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
        internal override bool HasArg { get { return ThenCode.HasArg || ElseCode.HasArg; } }
        protected override Size GetAdditionalTemporarySize() { return ThenCode.TemporarySize.Max(ElseCode.TemporarySize).Max(OutputSize) - OutputSize; }
        protected override FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ThenElse(this); }
        internal override void Visit(IVisitor visitor) { visitor.ThenElse(_condSize, ThenCode, ElseCode); }
        internal FiberItem ReCreate(CodeBase newThen, CodeBase newElse) { return new ThenElse(_condSize, newThen ?? ThenCode, newElse ?? ElseCode); }
    }
}