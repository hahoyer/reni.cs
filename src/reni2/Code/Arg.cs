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
using Reni.Type;

namespace Reni.Code
{
    /// <summary>
    ///     Arg is is used as a placeholder.
    /// </summary>
    internal sealed class Arg : FiberHead
    {
        private static int _nextObjectId;
        private readonly TypeBase _type;

        internal Arg(TypeBase type)
            : base(_nextObjectId++)
        {
            _type = type;
            StopByObjectId(1);
        }

        [Node]
        internal TypeBase Type { get { return _type; } }

        protected override Size GetSize() { return _type.Size; }
        protected override CodeArgs GetRefsImplementation() { return CodeArgs.Arg(); }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Arg(this); }
    }
}