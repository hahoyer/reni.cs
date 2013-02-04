#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Code element for a call that has been resolved as simple recursive call candidate. This implies, that the call is contained in the function called. It must not have any argument and should return nothing. It will be assembled as a jump to begin of function.
    /// </summary>
    sealed class RecursiveCallCandidate : FiberItem
    {
        readonly Size _refsSize;

        internal override Size InputSize { get { return _refsSize; } }

        internal override Size OutputSize { get { return Size.Zero; } }

        internal override void Visit(IVisitor visitor) { visitor.RecursiveCallCandidate(); }

        internal override CodeBase TryToCombineBack(TopFrameData precedingElement)
        {
            if((DeltaSize + precedingElement.Size).IsZero
                && precedingElement.Offset.IsZero)
                return new RecursiveCall();
            return base.TryToCombineBack(precedingElement);
        }

        internal RecursiveCallCandidate(Size refsSize) { _refsSize = refsSize; }
    }
}