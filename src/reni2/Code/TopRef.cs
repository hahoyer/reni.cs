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
using hw.Debug;
using Reni.Basics;

namespace Reni.Code
{
    sealed class TopRef : Ref
    {
        public TopRef(Size offset)
            : base(offset) { StopByObjectId(-46); }

        public TopRef()
            : this(Size.Zero) { }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override void Visit(IVisitor visitor) { visitor.TopRef(Offset); }
    }

    sealed class TopFrameRef : Ref
    {
        public TopFrameRef()
            : this(Size.Zero) { }

        public TopFrameRef(Size offset)
            : base(offset) { StopByObjectId(-40); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
        internal override void Visit(IVisitor visitor) { visitor.TopFrameRef(Offset); }
    }
}