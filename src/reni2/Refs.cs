using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;

namespace Reni
{
    /// <summary>
    /// Contains list of references to compiler environemnts.
    /// </summary>
    public class Refs : ReniObject
    {
        [Node]
        public List<Base> Data { get { return _data; } }

        private readonly List<Base> _data;
        private SizeArray _sizes;
        public bool IsPending { get { return _isPending; } }
        private bool _isPending = false;

        private Refs()
        {
            _data = new List<Base>();
            StopByObjectId(-441);
        }

        private Refs(Base e)
            : this()
        {
            _data.Add(e);
        }

        private Refs(List<Base> a, List<Base> b)
            : this(a)
        {
            foreach (Base e in b)
            {
                if (!_data.Contains(e))
                    _data.Add(e);
            }
        }

        private Refs(List<Base> a)
            : this()
        {
            _data.AddRange(a);
        }

        private Refs(bool isPending)
        {
            _isPending = isPending;
        }

        /// <summary>
        /// Creates the empty reference list
        /// </summary>
        /// <returns></returns>
        public static Refs None()
        {
            return new Refs();
        }

        /// <summary>
        /// Combine two set of refs
        /// </summary>
        /// <param name="refs"></param>
        /// <returns></returns>
        public Refs Pair(Refs refs)
        {
            if (IsPending || refs.IsPending)
                throw new NotSupportedException("Pair function not allowed for pending refs");
            if (refs.Count == 0)
                return this;
            if (Count == 0)
                return refs;
            return new Refs(_data, refs._data);
        }

        /// <summary>
        /// Create a simple environment reference
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Refs Context(Base e)
        {
            return new Refs(e);
        }

        /// <summary>
        /// Gets the size of each ref contained 
        /// </summary>
        [DumpData(false)]
        private SizeArray Sizes
        {
            get
            {
                if (_sizes == null)
                    _sizes = CalcSizes();
                return _sizes;
            }
        }

        /// <summary>
        /// Default dump of data
        /// </summary>
        /// <returns></returns>
        public override string DumpData()
        {
            if (IsPending)
                return "<pending>";

            string result = "";
            for (int i = 0; i < Count; i++)
            {
                if (i > 0)
                    result += "\n";
                result += Tracer.Dump(_data[i]);
            }
            return result;
        }

        /// <summary>
        /// DigitChain of refs
        /// </summary>
        public int Count
        {
            get
            {
                if (IsPending)
                    throw new NotSupportedException();
                return _data.Count;
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public Base this[int i] { get { return _data[i]; } }

        private SizeArray CalcSizes()
        {
            SizeArray result = new SizeArray();
            for (int i = 0, n = Count; i < n; i++)
                result.Add((this)[i].RefSize);
            return result;
        }

        /// <summary>
        /// removes parameter from coloection if contained
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Refs Without(Base e)
        {
            if (IsPending)
                throw new NotSupportedException();
            if (!_data.Contains(e))
                return this;
            List<Base> r = new List<Base>(_data);
            r.Remove(e);
            return new Refs(r);
        }

        /// <summary>
        /// obtain size
        /// </summary>
        public Size Size { get { return Sizes.Size; } }

        /// <summary>
        /// Gets a value indicating whether this instance is none.
        /// </summary>
        /// <value><c>true</c> if this instance is none; otherwise, <c>false</c>.</value>
        /// [created 05.06.2006 16:29]
        public bool IsNone { get { return Count == 0; } }

        /// <summary>
        /// Gets the pending.
        /// </summary>
        /// <value>The pending.</value>
        /// created 24.01.2007 22:12
        public static Refs Pending { get { return new Refs(true); } }

        /// <summary>
        /// Determines whether [contains] [the specified context].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified context]; otherwise, <c>false</c>.
        /// </returns>
        /// created 18.10.2006 00:12
        public bool Contains(Base context)
        {
            return _data.Contains(context);
        }

        /// <summary>
        /// Determines whether [contains] [the specified context].
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified context]; otherwise, <c>false</c>.
        /// </returns>
        /// created 18.10.2006 00:12
        public bool Contains(Refs other)
        {
            foreach (Base context in other._data)
                if (!Contains(context))
                    return false;

            return true;
        }

        /// <summary>
        /// Toes the code.
        /// </summary>
        /// created 06.11.2006 22:57
        internal Code.Base ToCode()
        {
            Code.Base result = Code.Base.CreateVoid();
            for (int i = 0; i < _data.Count; i++)
                result = result.CreateSequence(Code.Base.CreateContextRef((StructContainer) _data[i]));
            return result;
        }

        /// <summary>
        /// Replaces the refs for function body.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="endOfRefsCode">The endOfRefsCode.</param>
        /// <returns></returns>
        /// created 31.12.2006 18:47
        internal Code.Base ReplaceRefsForFunctionBody(Code.Base code, RefAlignParam refAlignParam, Code.Base endOfRefsCode)
        {
            Code.Base p = endOfRefsCode;
            Code.Base result = code;
            for (int i = 0; i < _data.Count; i++)
            {
                RefAlignParam unrefAlignment = _data[i].RefAlignParam;
                Tracer.Assert(unrefAlignment.IsEqual(refAlignParam));
                RefAlignParam unrefPtrAlignment = refAlignParam;
                    // To do; check if this is correct. Can be chaecked if we really have different alignment
                p = p.CreateRefPlus(unrefPtrAlignment, unrefPtrAlignment.RefSize*-1);
                Code.Base replacement = p.CreateDereference(unrefPtrAlignment, unrefAlignment.RefSize);
                result = result.ReplaceAbsoluteContextRef((StructContainer) _data[i], replacement);
            }
            return result;
        }
    }
}