using System;
using HWClassLibrary.Helper;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Base class for code replacements
    /// </summary>
    internal abstract class Base : Visitor<CodeBase>
    {
        private DictionaryEx<InternalRef, InternalRef> _internalRefs = new DictionaryEx<InternalRef, InternalRef>();

        internal override CodeBase Arg(Arg visitedObject) { return null; }

        internal override CodeBase InternalRef(InternalRef visitedObject)
        {
            return _internalRefs.Find(visitedObject, () => ReVisit(visitedObject));
        }

        internal override sealed CodeBase Child(CodeBase parent, LeafElement leafElement)
        {
            if(parent == null)
                return null;
            return parent.CreateChild(leafElement);
        }

        internal override sealed CodeBase Leaf(LeafElement leafElement) { return null; }

        internal override CodeBase Pair(Pair visitedObject, CodeBase left, CodeBase right)
        {
            if(left == null && right == null)
                return null;
            if(left == null)
                left = visitedObject.Left;
            if(right == null)
                right = visitedObject.Right;
            return left.CreateSequence(right);
        }

        internal override CodeBase ThenElse(ThenElse visitedObject, CodeBase condResult, CodeBase thenResult,
            CodeBase elseResult)
        {
            if(condResult == null && thenResult == null && elseResult == null)
                return null;
            if(condResult == null)
                condResult = visitedObject.CondCode;
            if(thenResult == null)
                thenResult = visitedObject.ThenCode;
            if(elseResult == null)
                elseResult = visitedObject.ElseCode;
            return condResult.CreateThenElse(thenResult, elseResult);
        }

        internal override Visitor<CodeBase> AfterCond(int objectId) { return this; }

        internal override Visitor<CodeBase> AfterThen(int objectId, Size theSize) { return this; }

        internal override Visitor<CodeBase> AfterElse(int objectId) { return this; }

        internal override CodeBase ContextRef(RefCode visitedObject) { return null; }

        internal InternalRef ReVisit(InternalRef visitedObject)
        {
            var newCode = visitedObject.Code.Visit(this);
            if (newCode == null)
                return null;
            return new InternalRef(visitedObject.RefAlignParam, newCode, visitedObject.DestructorCode);
        }
    }
}