using Reni.Code;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Base class for code replacements
    /// </summary>
    public abstract class Base : Visitor<Code.Base>
    {
        /// <summary>
        /// Bits the array.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 24.09.2006 17:05
        public Code.Base BitArray(BitArray visitedObject)
        {
            return null;
        }

        /// <summary>
        /// Tops the ref.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 02.10.2006 20:42
        public Code.Base TopRef(TopRef visitedObject)
        {
            return null;
        }

        /// <summary>
        /// Childs the specified visited object.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        /// created 03.10.2006 02:50
        public Code.Base Child(Child visitedObject, Code.Base parent)
        {
            if (parent == null)
                return null;
            return visitedObject.ReCreate(parent);
        }

        /// <summary>
        /// Args the specified visited object.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 24.09.2006 20:17
        public override Code.Base Arg(Arg visitedObject)
        {
            return null;
        }

        /// <summary>
        /// Childs the specified parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="leafElement">The element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:11
        sealed public override Code.Base Child(Code.Base parent, LeafElement leafElement)
        {
            if (parent == null)
                return null;
            return parent.CreateChild(leafElement);
        }

        /// <summary>
        /// Leafs the specified leaf element.
        /// </summary>
        /// <param name="leafElement">The leaf element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:22
        sealed public override Code.Base Leaf(LeafElement leafElement)
        {
            return null;
        }

        /// <summary>
        /// Sequences the specified visited object.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 03.10.2006 01:39
        /// created 03.10.2006 01:39
        public override Code.Base Pair(Pair visitedObject, Code.Base left, Code.Base right)
        {
            if (left == null && right == null)
                return null;
            if (left == null) left = visitedObject.Left;
            if (right == null) right = visitedObject.Right;
            return left.CreateSequence(right);
        }

        /// <summary>
        /// Thens the else.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="condResult">The cond result.</param>
        /// <param name="thenResult">The then result.</param>
        /// <param name="elseResult">The else result.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:54
        public override Code.Base ThenElse(ThenElse visitedObject, Code.Base condResult, Code.Base thenResult,
                                           Code.Base elseResult)
        {
            if(condResult==null && thenResult == null && elseResult == null)
                return null;
            if (condResult == null) condResult = visitedObject.CondCode;
            if (thenResult == null) thenResult = visitedObject.ThenCode;
            if (elseResult == null) elseResult = visitedObject.ElseCode;
            return condResult.CreateThenElse(thenResult, elseResult);
        }

        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        public override Visitor<Code.Base> AfterCond(int objectId)
        {
            return this;
        }

        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <param name="theSize">The size.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        public override Visitor<Code.Base> AfterThen(int objectId, Size theSize)
        {
            return this;
        }

        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        public override Visitor<Code.Base> AfterElse(int objectId)
        {
            return this;
        }

    }
}