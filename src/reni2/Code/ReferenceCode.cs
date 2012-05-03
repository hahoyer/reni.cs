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

using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     ContextAtPosition reference, should be replaced
    /// </summary>
    internal sealed class ReferenceCode : FiberHead
    {
        private readonly IReferenceInCode _context;
        private static int _nextObjectId;

        internal ReferenceCode(IReferenceInCode context)
            : base(_nextObjectId++)
        {
            _context = context;
            StopByObjectId(-10);
        }

        [Node]
        internal IReferenceInCode Context { get { return _context; } }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }

        [DisableDump]
        internal override bool IsRelativeReference { get { return false; } }

        protected override CodeArgs GetRefsImplementation() { return CodeArgs.Create(_context); }

        protected override Size GetSize() { return RefAlignParam.RefSize; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
        internal override void Visit(IVisitor visitor) { visitor.ReferenceCode(Context); }

        public override string DumpData() { return _context.DumpShort(); }
    }
}