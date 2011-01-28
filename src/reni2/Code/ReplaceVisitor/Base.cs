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

        internal override CodeBase Arg(Arg visitedObject) { return null; }
        internal override CodeBase ContextRef(ReferenceCode visitedObject) { return null; }
        internal override CodeBase LocalReference(LocalReference visitedObject) { return _internalRefs.Find(visitedObject); }

        protected override CodeBase List(List visitedObject, IEnumerable<CodeBase> newList)
        {
            if (newList.All(x => x == null))
                return null;
            return Code.List.Create(newList.Select((x, i) => x ?? visitedObject.Data[i]));
        }

        protected override FiberItem ThenElse(ThenElse visitedObject, CodeBase newThen, CodeBase newElse)
        {
            if (newThen == null && newElse == null)
                return null;
            return visitedObject.ReCreate(newThen, newElse);
        }

        protected override CodeBase Fiber(Fiber visitedObject, CodeBase newHead, FiberItem[] newItems)
        {
            if (newHead == null && newItems.All(x => x == null))
                return null;

            return visitedObject.ReCreate(newHead, newItems);
        }

        internal override CodeBase Default() { return null; }

        internal LocalReference ReVisit(LocalReference visitedObject)
        {
            var newCode = visitedObject.Code.Visit(this);
            if(newCode == null)
                return null;
            return newCode.LocalReference(visitedObject.RefAlignParam, visitedObject.DestructorCode);
        }
    }
}