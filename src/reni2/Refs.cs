using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;

namespace Reni
{
    /// <summary>
    /// Contains list of references to compiler environemnts.
    /// </summary>
    [Serializable]
    internal sealed class Refs : ReniObject
    {
        private static int _nextId;
        private readonly List<IRefInCode> _data;
        private SizeArray _sizesCache;

        private Refs()
            : base(_nextId++)
        {
            _data = new List<IRefInCode>();
            StopByObjectId(-49);
        }

        private Refs(IRefInCode context)
            : this() { Add(context); }

        private Refs(IEnumerable<IRefInCode> a, IEnumerable<IRefInCode> b)
            : this()
        {
            AddRange(a);
            AddRange(b);
        }

        private Refs(IEnumerable<IRefInCode> a)
            : this() { AddRange(a); }

        private void AddRange(IEnumerable<IRefInCode> a)
        {
            foreach(var e in a)
                Add(e);
        }

        private void Add(IRefInCode e)
        {
            if(!_data.Contains(e))
                _data.Add(e);
        }

        [Node]
        public List<IRefInCode> Data { get { return _data; } }

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

        public int Count { get { return _data.Count; } }

        public IRefInCode this[int i] { get { return _data[i]; } }
        public Size Size { get { return Sizes.Size; } }
        public bool IsNone { get { return Count == 0; } }

        public static Refs None() { return new Refs(); }

        public Refs CreateSequence(Refs refs)
        {
            if(refs.Count == 0)
                return this;
            if(Count == 0)
                return refs;
            return new Refs(_data, refs._data);
        }

        internal static Refs Create(IRefInCode refInCode) { return new Refs(refInCode); }

        public override string DumpData()
        {
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
                result.Add((this)[i].RefAlignParam.RefSize);
            return result;
        }

        public Refs Without(IRefInCode e)
        {
            if(!_data.Contains(e))
                return this;
            var r = new List<IRefInCode>(_data);
            r.Remove(e);
            return new Refs(r);
        }

        public Refs Without(Refs other)
        {
            var result = this;
            foreach(var refInCode in other._data)
                result = result.Without(refInCode);
            return result;
        }

        public bool Contains(IRefInCode context) { return _data.Contains(context); }

        public bool Contains(Refs other)
        {
            foreach(var context in other._data)
                if(!Contains(context))
                    return false;

            return true;
        }

        internal CodeBase ToCode()
        {
            return _data
                .Aggregate(CodeBase.CreateVoid(), (current, t) => current.CreateSequence(CodeBase.Create(t)));
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
                p = p.CreateRefPlus(unrefPtrAlignment, unrefPtrAlignment.RefSize * -1, "ReplaceRefsForFunctionBody");
                var replacement = p.CreateDereference(unrefPtrAlignment, unrefAlignment.RefSize);
                result = result.ReplaceAbsolute(_data[i], ()=>replacement);
            }
            return result;
        }

        public static Refs operator +(Refs x, Refs y) { return x.CreateSequence(y); }
        public static Refs operator -(Refs x, Refs y) { return x.Without(y); }
        public static Refs operator -(Refs x, IRefInCode y) { return x.Without(y); }
    }
}