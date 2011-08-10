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
    internal abstract class FiberItem : ReniObject, IFormalCodeItem
    {
        private static int _nextObjectId;
        private static string _newCombinedReason;
        private readonly string _reason;

        [DisableDump]
        internal string ReasonForCombine { get { return _reason == "" ? DumpShortForDebug() : _reason; } }

        [DisableDump]
        private string NewCombinedReason
        {
            get
            {
                if(_newCombinedReason == null)
                    return "";
                return _newCombinedReason;
            }
            set
            {
                Tracer.Assert((_newCombinedReason == null) != (value == null));
                _newCombinedReason = value;
                ;
            }
        }

        [DumpExcept("")]
        [EnableDump]
        internal string Reason { get { return _reason; } }

        protected FiberItem(int objectId, string reason = null)
            : base(objectId) { _reason = reason ?? NewCombinedReason; }

        protected FiberItem(string reason = null)
            : this(_nextObjectId++, reason) { }

        [DisableDump]
        internal abstract Size InputSize { get; }

        [DisableDump]
        internal abstract Size OutputSize { get; }

        [DisableDump]
        internal Size DeltaSize { get { return OutputSize - InputSize; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + DumpSignature; } }

        [DisableDump]
        private string DumpSignature { get { return "(" + InputSize + "==>" + OutputSize + ")"; } }

        [DisableDump]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        [DisableDump]
        internal Refs Refs { get { return GetRefsImplementation(); } }

        [DisableDump]
        internal virtual bool HasArg { get { return false; } }

        internal string ReversePolish(Size top) { return CSharpCodeSnippet(top) + ";\n"; }

        protected virtual string CSharpCodeSnippet(Size top)
        {
            NotImplementedMethod(top);
            return null;
        }

        internal FiberItem[] TryToCombine(FiberItem subsequentElement)
        {
            NewCombinedReason = ReasonForCombine + " " + subsequentElement.ReasonForCombine;
            var result = TryToCombineImplementation(subsequentElement);
            NewCombinedReason = null;
            return result;
        }

        protected virtual FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return null; }

        internal virtual CodeBase TryToCombineBack(BitArray precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopFrameRef precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopData precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopFrameData precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopRef precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(LocalVariableReference precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(LocalVariableAccess precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitArrayPrefixOp precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitCast preceding) { return null; }
        internal virtual FiberItem[] TryToCombineBack(Dereference preceding) { return null; }
        internal virtual FiberItem[] TryToCombineBack(RefPlus precedingElement) { return null; }

        internal FiberItem Visit<TResult>(Visitor<TResult> actual) { return VisitImplementation(actual); }

        protected virtual FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) { return null; }

        protected abstract void Execute(IFormalMaschine formalMaschine);

        void IFormalCodeItem.Execute(IFormalMaschine formalMaschine) { Execute(formalMaschine); }

        protected virtual Refs GetRefsImplementation() { return Refs.ArgLess(); }
    }

    internal interface IFormalCodeItem
    {
        void Execute(IFormalMaschine formalMaschine);
        string Dump();
    }
}