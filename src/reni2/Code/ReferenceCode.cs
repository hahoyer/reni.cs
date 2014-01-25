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
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     ContextAtPosition reference, should be replaced
    /// </summary>
    sealed class ReferenceCode : FiberHead
    {
        readonly IContextReference _context;
        static int _nextObjectId;

        internal ReferenceCode(IContextReference context)
            : base(_nextObjectId++)
        {
            _context = context;
            StopByObjectId(-5);
        }

        [Node]
        internal IContextReference Context { get { return _context; } }

        [DisableDump]
        internal override bool IsRelativeReference { get { return false; } }

        protected override CodeArgs GetRefsImplementation() { return CodeArgs.Create(_context); }

        protected override Size GetSize() { return _context.Size; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
        internal override void Visit(IVisitor visitor) { visitor.ReferenceCode(Context); }

        public override string DumpData() { return _context.NodeDump(); }
    }
}