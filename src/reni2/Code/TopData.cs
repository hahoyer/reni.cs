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
    /// <summary>
    ///     Combination of TopRef and Unref
    /// </summary>
    [Serializable]
    internal sealed class TopData : Top
    {
        public TopData(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
            : base(refAlignParam, offset, size, dataSize) { StopByObjectId(-110); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            var fiber = subsequentElement.TryToCombineBack(this);
            if(fiber == null)
                return null;
            return fiber;
        }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopData(Offset, Size, DataSize); }

        protected override string CSharpString() { return CSharpGenerator.TopData(Offset, GetSize()); }
    }

    /// <summary>
    ///     Combination of TopFrameRef and Unref
    /// </summary>
    [Serializable]
    internal sealed class TopFrameData : Top
    {
        public TopFrameData(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
            : base(refAlignParam, offset, size, dataSize) { StopByObjectId(544); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            var fiber = subsequentElement.TryToCombineBack(this);
            if(fiber == null)
                return null;
            return fiber;
        }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopFrameData(Offset, Size, DataSize); }

        protected override string CSharpString() { return CSharpGenerator.TopFrame(Offset, GetSize()); }
    }
}