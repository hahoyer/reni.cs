using System;
using System.Collections.Generic;
using hw.Forms;
using hw.Helper;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni
{
    /// <summary>
    ///     Contains list of references to compiler environemnts.
    /// </summary>
    sealed class CodeArgs : DumpableObject, ITreeNodeSupport
    {
        static int _nextId;
        readonly List<IContextReference> _data;
        SizeArray _sizesCache;
        readonly ValueCache<IContextReference[]> _sortedDataCache;
        public static int NextOrder;

        CodeArgs()
            : base(_nextId++)
        {
            _data = new List<IContextReference>();
            _sortedDataCache = new ValueCache<IContextReference[]>(ObtainSortedData);
            StopByObjectIds(-10);
        }

        CodeArgs(IContextReference context)
            : this() { Add(context); }

        CodeArgs(IEnumerable<IContextReference> a, IEnumerable<IContextReference> b)
            : this()
        {
            AddRange(a);
            AddRange(b);
        }

        CodeArgs(IEnumerable<IContextReference> a)
            : this() { AddRange(a); }


        void AddRange(IEnumerable<IContextReference> a)
        {
            foreach(var e in a)
                Add(e);
        }

        void Add(IContextReference e)
        {
            if(!_data.Contains(e))
                _data.Add(e);
        }

        [SmartNode]
        public List<IContextReference> Data => _data;

        [DisableDump]
        SizeArray Sizes => _sizesCache ?? (_sizesCache = CalculateSizes());

        internal bool HasArg => Contains(CodeArg.Instance);
        public int Count => _data.Count;

        IContextReference this[int i] => _data[i];
        public Size Size => Sizes.Size;
        public bool IsNone => Count == 0;
        IContextReference[] SortedData => _sortedDataCache.Value;

        internal static CodeArgs Void() => new CodeArgs();
        internal static CodeArgs Arg() => new CodeArgs(CodeArg.Instance);

        public CodeArgs Sequence(CodeArgs codeArgs)
        {
            if(codeArgs.Count == 0)
                return this;
            if(Count == 0)
                return codeArgs;
            return new CodeArgs(_data, codeArgs._data);
        }

        internal static CodeArgs Create(IContextReference contextReference)
            => new CodeArgs(contextReference);

        protected override string GetNodeDump()
        {
            if(Count > 5)
                return base.GetNodeDump() + " Count = " + Count;
            return "{"
                + _data.Select(contextReference => contextReference.NodeDump()).Stringify(",") + "}";
        }

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

        SizeArray CalculateSizes()
        {
            var result = new SizeArray();
            for(var i = 0; i < Count; i++)
                result.Add(_data[i].Size());
            return result;
        }

        public CodeArgs Without(IContextReference e)
        {
            if(!_data.Contains(e))
                return this;
            var r = new List<IContextReference>(_data);
            r.Remove(e);
            return new CodeArgs(r);
        }

        IContextReference[] ObtainSortedData()
            => _data
                .OrderBy(codeArg => codeArg.Order)
                .ToArray();

        public CodeArgs WithoutArg() => Without(CodeArg.Instance);

        CodeArgs Without(CodeArgs other)
            => other
                ._data
                .Aggregate(this, (current, refInCode) => current.Without(refInCode));

        public bool Contains(IContextReference context) => _data.Contains(context);
        public bool Contains(CodeArgs other)
        {
            if(Count < other.Count)
                return false;

            for(int i = 0, j = 0; i < Count; i++)
            {
                var delta = SortedData[i].Order - other.SortedData[j].Order;

                if(delta > 0)
                    return false;

                if(delta == 0)
                {
                    j++;
                    if(j == other.Count)
                        return true;
                }
            }
            return false;
        }
        public bool IsEqual(CodeArgs other)
        {
            if(Count != other.Count)
                return false;

            for(var i = 0; i < Count; i++)
                if(SortedData[i].Order != other.SortedData[i].Order)
                    return false;

            return true;
        }

        internal CodeBase ToCode()
            => _data
                .Aggregate(CodeBase.Void, (current, t) => current + CodeBase.ReferenceCode(t));

        internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, CodeBase codeArgsReference)
        {
            var trace = ObjectId == -1;
            StartMethodDump(trace, code, codeArgsReference);
            try
            {
                var refSize = Root.DefaultRefAlignParam.RefSize;
                var reference = codeArgsReference.ReferencePlus(refSize * _data.Count);
                var result = code;
                foreach(var referenceInCode in _data)
                {
                    Dump("reference", reference);
                    BreakExecution();
                    reference = reference.ReferencePlus(refSize * -1);
                    result = result.ReplaceAbsolute
                        (referenceInCode, () => reference.DePointer(refSize));
                    Dump("result", result);
                }
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        public static CodeArgs operator +(CodeArgs x, CodeArgs y) => x.Sequence(y);
        public static CodeArgs operator -(CodeArgs x, CodeArgs y) => x.Without(y);
        public static CodeArgs operator -(CodeArgs x, IContextReference y) => x.Without(y);
        IEnumerable<TreeNode> ITreeNodeSupport.CreateNodes() => _data.CreateNodes();

        sealed class CodeArg : Singleton<CodeArg, DumpableObject>, IContextReference
        {
            int IContextReference.Order => -1;
            protected override string GetNodeDump() => "CodeArg";
        }
    }
}