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
        private readonly List<IReferenceInCode> _data;
        private SizeArray _sizesCache;

        private Refs()
            : base(_nextId++)
        {
            _data = new List<IReferenceInCode>();
            StopByObjectId(-49);
        }

        private Refs(IReferenceInCode context)
            : this() { Add(context); }

        private Refs(IEnumerable<IReferenceInCode> a, IEnumerable<IReferenceInCode> b)
            : this()
        {
            AddRange(a);
            AddRange(b);
        }

        private Refs(IEnumerable<IReferenceInCode> a)
            : this() { AddRange(a); }

        private void AddRange(IEnumerable<IReferenceInCode> a)
        {
            foreach(var e in a)
                Add(e);
        }

        private void Add(IReferenceInCode e)
        {
            if(!_data.Contains(e))
                _data.Add(e);
        }

        [Node]
        public List<IReferenceInCode> Data { get { return _data; } }

        [IsDumpEnabled(false)]
        private SizeArray Sizes
        {
            get
            {
                if(_sizesCache == null)
                    _sizesCache = CalculateSizes();
                return _sizesCache;
            }
        }

        public int Count { get { return _data.Count; } }

        public IReferenceInCode this[int i] { get { return _data[i]; } }
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

        internal static Refs Create(IReferenceInCode referenceInCode) { return new Refs(referenceInCode); }

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

        private SizeArray CalculateSizes()
        {
            var result = new SizeArray();
            for(var i = 0; i < Count; i++)
                result.Add((this)[i].RefAlignParam.RefSize);
            return result;
        }

        public Refs Without(IReferenceInCode e)
        {
            if(!_data.Contains(e))
                return this;
            var r = new List<IReferenceInCode>(_data);
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

        public bool Contains(IReferenceInCode context) { return _data.Contains(context); }

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
                .Aggregate(CodeBase.Void(), (current, t) => current.Sequence(CodeBase.ReferenceInCode(t)));
        }

        internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, RefAlignParam refAlignParam, CodeBase endOfRefsCode)
        {
            var p = endOfRefsCode.AddToReference(refAlignParam, refAlignParam.RefSize * -_data.Count, "ReplaceRefsForFunctionBody");
            var result = code;
            for(var i = 0; i < _data.Count; i++)
            {
                var unrefAlignment = _data[i].RefAlignParam;
                Tracer.Assert(unrefAlignment.IsEqual(refAlignParam));
                var unrefPtrAlignment = refAlignParam;
                var replacement = p.Dereference(unrefPtrAlignment, unrefAlignment.RefSize);
                result = result.ReplaceAbsolute(_data[i], ()=>replacement);
                p = p.AddToReference(unrefPtrAlignment, unrefPtrAlignment.RefSize, "." + i);
            }
            return result;
        }

        public static Refs operator +(Refs x, Refs y) { return x.CreateSequence(y); }
        public static Refs operator -(Refs x, Refs y) { return x.Without(y); }
        public static Refs operator -(Refs x, IReferenceInCode y) { return x.Without(y); }
    }
}