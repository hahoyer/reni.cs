using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Context;

namespace Reni
{
    /// <summary>
    /// Contains list of references to compiler environemnts.
    /// </summary>
    internal sealed class Refs : ReniObject
    {
        private readonly List<ContextBase> _data;
        private readonly bool _isPending;
        private SizeArray _sizesCache;

        private Refs()
        {
            _data = new List<ContextBase>();
            StopByObjectId(-441);
        }

        private Refs(ContextBase e): this()
        {
            Add(e);
        }

        private Refs(IEnumerable<ContextBase> a, IEnumerable<ContextBase> b): this()
        {
            AddRange(a);
            AddRange(b);
        }

        private Refs(IEnumerable<ContextBase> a) : this()
        {
            AddRange(a);
        }

        private Refs(bool isPending)
        {
            _isPending = isPending;
        }

        private void AddRange(IEnumerable<ContextBase> a)
        {
            foreach (var e in a)
                Add(e);
        }

        private void Add(ContextBase e)
        {
            var trace = e.ObjectId == 6;
            StartMethodDumpWithBreak(trace,e);
            if (!_data.Contains(e))
                _data.Add(e);
            ReturnMethodDump(trace);
        }

        [Node]
        public List<ContextBase> Data { get { return _data; } }
        public bool IsPending { get { return _isPending; } }

        [DumpData(false)]
        private SizeArray Sizes
        {
            get
            {
                if(_sizesCache == null)
                    _sizesCache = CalcSizes();
                return _sizesCache;
            }
        }

        public int Count
        {
            get
            {
                if(IsPending)
                    throw new NotSupportedException();
                return _data.Count;
            }
        }

        public ContextBase this[int i] { get { return _data[i]; } }
        public Size Size { get { return Sizes.Size; } }
        public bool IsNone { get { return Count == 0; } }
        public static Refs Pending { get { return new Refs(true); } }

        public static Refs None()
        {
            return new Refs();
        }

        public Refs Pair(Refs refs)
        {
            if(IsPending || refs.IsPending)
                throw new NotSupportedException("Pair function not allowed for pending refs");
            if(refs.Count == 0)
                return this;
            if(Count == 0)
                return refs;
            return new Refs(_data, refs._data);
        }

        public static Refs Context(ContextBase e)
        {
            return new Refs(e);
        }

        public override string DumpData()
        {
            if(IsPending)
                return "<pending>";

            var result = "";
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    result += "\n";
                result += Tracer.Dump(_data[i]);
            }
            return result;
        }

        private SizeArray CalcSizes()
        {
            var result = new SizeArray();
            for(int i = 0, n = Count; i < n; i++)
                result.Add((this)[i].RefSize);
            return result;
        }

        public Refs Without(ContextBase e)
        {
            if(IsPending)
                throw new NotSupportedException();
            if(!_data.Contains(e))
                return this;
            var r = new List<ContextBase>(_data);
            r.Remove(e);
            return new Refs(r);
        }

        public bool Contains(ContextBase context)
        {
            return _data.Contains(context);
        }

        public bool Contains(Refs other)
        {
            foreach(var context in other._data)
                if(!Contains(context))
                    return false;

            return true;
        }

        internal CodeBase ToCode()
        {
            var result = CodeBase.CreateVoid();
            for(var i = 0; i < _data.Count; i++)
                result = result.CreateSequence(CodeBase.CreateContextRef((Struct.Context) _data[i]));
            return result;
        }

        internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, RefAlignParam refAlignParam, CodeBase endOfRefsCode)
        {
            var p = endOfRefsCode;
            var result = code;
            for(var i = 0; i < _data.Count; i++)
            {
                var unrefAlignment = _data[i].RefAlignParam;
                Tracer.Assert(unrefAlignment.IsEqual(refAlignParam));
                var unrefPtrAlignment = refAlignParam;
                // To do; check if this is correct. Can be chaecked if we really have different alignment
                p = p.CreateRefPlus(unrefPtrAlignment, unrefPtrAlignment.RefSize*-1);
                var replacement = p.CreateDereference(unrefPtrAlignment, unrefAlignment.RefSize);
                result = result.ReplaceAbsoluteContextRef((Struct.Context) _data[i], replacement);
            }
            return result;
        }
    }
}