using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Base class for code leafs
    /// </summary>
    abstract public class LeafElement: ReniObject
    {
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public abstract Size Size{get;}

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// created 06.10.2006 00:00
        [DumpData(false)]
        public virtual bool IsEmpty { get { return false; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        [DumpData(false)]
        public abstract Size DeltaSize{ get;}

        /// <summary>
        /// Statementses the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 11.10.2006 22:32
        public string Statements(StorageDescriptor start)
        {
            string result = Format(start);
            if (result != "")
                result += "; // "+ CommentDump + "\n";
            return result;
        }

        private string CommentDump
        {
            get
            {
                return GetType().Name + " " + ObjectId;
            }
        }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 20:03
        [DumpData(false)]
        virtual public RefAlignParam RefAlignParam { get { return null; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected virtual string Format(StorageDescriptor start)
        {
            NotImplementedMethod(start);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:18
        virtual public LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine a leaf element with a preceding <see cref="Dereference"/> element.
        /// </summary>
        /// <param name="precedingElement">the preceding element.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:25
        virtual public LeafElement TryToCombineBack(Dereference precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine a leaf element with a preceding <see cref="TopRef"/> element.
        /// </summary>
        /// <param name="precedingElement">the preceding element.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:38
        virtual public LeafElement TryToCombineBack(TopRef precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine a leaf element with a preceding <see cref="BitCast"/> element.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.11.2006 19:13
        virtual public LeafElement TryToCombineBack(BitCast precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 03.01.2007 22:43
        virtual public LeafElement TryToCombineBack(FrameRef precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 03:50
        virtual public LeafElement TryToCombineBack(BitArray precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 15:07
        virtual public LeafElement TryToCombineBack(TopData precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 15:07
        virtual public LeafElement TryToCombineBack(TopFrame precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 15:57
        virtual public LeafElement TryToCombineBack(BitArrayOp precedingElement)
        {
            return null;
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 17:55
        virtual public LeafElement TryToCombineBack(RefPlus precedingElement)
        {
            return null;
        }

        virtual public LeafElement Visit(ReplacePrimitiveRecursivity replacePrimitiveRecursivity)
        {
            return this;
        }

        virtual internal BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }
    }
}