using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Validation;

namespace Reni.Code
{
    abstract class FiberItem : DumpableObject, IFormalCodeItem
    {
        static int _nextObjectId;
        static string _newCombinedReason;
        readonly string _reason;

        [DisableDump]
        string ReasonForCombine => _reason == "" ? NodeDumpForDebug() : _reason;

        [DisableDump]
        static string NewCombinedReason
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
            }
        }

        [EnableDumpExcept("")]
        [EnableDump]
        internal string Reason => _reason;

        protected FiberItem(int objectId, string reason = null)
            : base(objectId) { _reason = reason ?? NewCombinedReason; }

        protected FiberItem(string reason = null)
            : this(_nextObjectId++, reason) { }

        [DisableDump]
        internal abstract Size InputSize { get; }

        [DisableDump]
        internal abstract Size OutputSize { get; }

        [DisableDump]
        internal Size DeltaSize => OutputSize - InputSize;

        protected override string GetNodeDump() => base.GetNodeDump() + DumpSignature;

        [DisableDump]
        string DumpSignature => "(" + InputSize + "==>" + OutputSize + ")";

        [DisableDump]
        internal CodeArgs CodeArgs => GetRefsImplementation();

        [DisableDump]
        internal virtual bool HasArg => false;

        [DisableDump]
        internal Size TemporarySize => OutputSize + GetAdditionalTemporarySize();

        [DisableDump]
        internal virtual IEnumerable<Issue> Issues => Issue.Empty;

        protected virtual Size GetAdditionalTemporarySize() => Size.Zero;

        internal FiberItem[] TryToCombine(FiberItem subsequentElement)
        {
            NewCombinedReason = ReasonForCombine + " " + subsequentElement.ReasonForCombine;
            var result = TryToCombineImplementation(subsequentElement);
            NewCombinedReason = null;
            return result;
        }

        protected virtual FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) => null;

        internal virtual CodeBase TryToCombineBack(BitArray precedingElement) => null;
        internal virtual CodeBase TryToCombineBack(TopFrameRef precedingElement) => null;
        internal virtual CodeBase TryToCombineBack(TopData precedingElement) => null;
        internal virtual CodeBase TryToCombineBack(TopFrameData precedingElement) => null;
        internal virtual CodeBase TryToCombineBack(TopRef precedingElement) => null;
        internal virtual CodeBase TryToCombineBack(List precedingElement) => null;
        internal virtual CodeBase TryToCombineBack(LocalReference precedingElement) => null;
        internal virtual FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement) => null;
        internal virtual FiberItem[] TryToCombineBack(BitArrayPrefixOp precedingElement) => null;
        internal virtual FiberItem[] TryToCombineBack(BitCast preceding) => null;
        internal virtual FiberItem[] TryToCombineBack(DePointer preceding) => null;
        internal virtual FiberItem[] TryToCombineBack(ReferencePlusConstant precedingElement) => null;

        internal FiberItem Visit<TResult>(Visitor<TResult> actual) => VisitImplementation(actual);

        protected virtual FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) => null;

        internal abstract void Visit(IVisitor visitor);

        void IFormalCodeItem.Visit(IVisitor visitor) => Visit(visitor);

        protected virtual CodeArgs GetRefsImplementation() => CodeArgs.Void();
    }

    interface IFormalCodeItem
    {
        void Visit(IVisitor visitor);
        string Dump();
    }
}