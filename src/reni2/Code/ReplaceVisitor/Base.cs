using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    ///     Base class for code replacements
    /// </summary>
    internal abstract class Base : Visitor<CodeBase>
    {
        private readonly FunctionCache<LocalReference, LocalReference> _internalRefs;

        protected Base(int objectId)
            : base(objectId) { _internalRefs = new FunctionCache<LocalReference, LocalReference>(ReVisit); }

        internal override CodeBase Arg(Arg visitedObject) { return null; }
        internal override CodeBase ContextRef(ReferenceCode visitedObject) { return null; }
        internal override CodeBase BitArray(BitArray visitedObject) { return null; }
        internal override CodeBase Default(CodeBase codeBase) { return null; }

        internal override CodeBase LocalReference(LocalReference visitedObject) { return _internalRefs[visitedObject]; }

        protected override CodeBase List(List visitedObject, IEnumerable<CodeBase> newList)
        {
            var newListAsArray = newList.ToArray();
            if(newListAsArray.All(x => x == null))
                return null;
            var newListCompleted = newListAsArray
                .Select((x, i) => x ?? visitedObject.Data[i])
                .Where(x=>!x.IsEmpty)
                .ToArray();

            switch(newListCompleted.Length)
            {
                case 0:
                    return CodeBase.Void;
                case 1:
                    return newListCompleted[0];
            }
            
            return CodeBase.List(newListCompleted);
        }

        protected override FiberItem ThenElse(ThenElse visitedObject, CodeBase newThen, CodeBase newElse)
        {
            if(newThen == null && newElse == null)
                return null;
            return visitedObject.ReCreate(newThen, newElse);
        }

        protected override CodeBase Fiber(Fiber visitedObject, CodeBase newHead, FiberItem[] newItems)
        {
            if(newHead == null && newItems.All(x => x == null))
                return null;

            return visitedObject.ReCreate(newHead, newItems);
        }

        internal LocalReference ReVisit(LocalReference visitedObject)
        {
            var newCode = visitedObject.Code.Visit(this);
            if(newCode == null)
                return null;
            return newCode.LocalReference(visitedObject.DestructorCode);
        }
    }
}