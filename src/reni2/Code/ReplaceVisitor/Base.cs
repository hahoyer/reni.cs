using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Base class for code replacements
    /// </summary>
    internal abstract class Base : Visitor<CodeBase>
    {
        private readonly DictionaryEx<LocalReference, LocalReference> _internalRefs;
        
        protected Base(int objectId):base(objectId) { _internalRefs = new DictionaryEx<LocalReference, LocalReference>(ReVisit); }
        protected Base() { _internalRefs = new DictionaryEx<LocalReference, LocalReference>(ReVisit); }

        internal override CodeBase Arg(Arg visitedObject) { return null; }
        internal override CodeBase ContextRef(ReferenceCode visitedObject) { return null; }
        internal override CodeBase LocalReference(LocalReference visitedObject) { return _internalRefs.Find(visitedObject); }

        protected override Visitor<CodeBase> AfterCond() { return this; }
        protected override Visitor<CodeBase> AfterElse() { return this; }

        protected override CodeBase List(List visitedObject, List<CodeBase> newList)
        {
            if (newList.All(x => x == null))
                return null;
            return Code.List.Create(newList.Select((x, i) => x ?? visitedObject.Data[i]));
        }

        protected override CodeBase LocalVariables(LocalVariables visitedObject, List<CodeBase> newList)
        {
            if (newList.All(x => x == null))
                return null;
            return new LocalVariables(visitedObject.HolderNamePattern,newList.Select((x, i) => x ?? visitedObject.Data[i]));
        }

        protected override Visitor<CodeBase> AfterThen(Size theSize) { return this; }

        protected override CodeBase Fiber(Fiber visitedObject, CodeBase head)
        {
            return head == null ? null : head.CreateFiber(visitedObject.FiberItems);
        }

        internal override CodeBase Default() { return null; }

        internal LocalReference ReVisit(LocalReference visitedObject)
        {
            var newCode = visitedObject.Code.Visit(this);
            if(newCode == null)
                return null;
            return new LocalReference(visitedObject.RefAlignParam, newCode, visitedObject.DestructorCode);
        }
    }
}