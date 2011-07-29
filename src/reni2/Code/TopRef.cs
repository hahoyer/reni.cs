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
using Reni.Basics;

namespace Reni.Code
{
    [Serializable]
    internal sealed class TopRef : Ref
    {
        public TopRef(RefAlignParam refAlignParam, Size offset)
            : base(refAlignParam, offset) { StopByObjectId(-64); }

        public TopRef(RefAlignParam refAlignParam)
            : this(refAlignParam, Size.Zero) { }

        protected override string CSharpString(Size top) { return CSharpGenerator.TopRef(top, Size); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopRef(Offset, Size); }
    }

    [Serializable]
    internal sealed class TopFrameRef : Ref
    {
        public TopFrameRef(RefAlignParam refAlignParam)
            : this(refAlignParam, Size.Zero) { }

        public TopFrameRef(RefAlignParam refAlignParam, Size offset)
            : base(refAlignParam, offset) { StopByObjectId(-46); }

        protected override string CSharpString() { return CSharpGenerator.CreateFrameRef(RefAlignParam, Offset); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopFrameRef(Offset, Size); }
    }
}