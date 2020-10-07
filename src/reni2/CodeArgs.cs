using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni
{
    /// <summary>
    ///     Contains list of references to compiler environments.
    /// </summary>
    sealed class Closures : DumpableObject
    {
        sealed class CodeArg : Singleton<CodeArg, DumpableObject>, IContextReference
        {
            int IContextReference.Order => -1;
            protected override string GetNodeDump() => "CodeArg";
        }

        public static int NextOrder;
        static int NextId;

        [SmartNode]
        [DisableDump]
        public List<IContextReference> Data { get; }

        SizeArray SizesCache;
        readonly ValueCache<IContextReference[]> SortedDataCache;

        Closures()
            : base(NextId++)
        {
            Data = new List<IContextReference>();
            SortedDataCache = new ValueCache<IContextReference[]>(ObtainSortedData);
            StopByObjectIds(-10);
        }

        Closures(IContextReference context)
            : this()
            => Add(context);

        Closures(IEnumerable<IContextReference> a, IEnumerable<IContextReference> b)
            : this()
        {
            AddRange(a);
            AddRange(b);
        }

        Closures(IEnumerable<IContextReference> a)
            : this()
            => AddRange(a);

        [DisableDump]
        SizeArray Sizes => SizesCache ??= CalculateSizes();

        internal bool HasArg => Contains(CodeArg.Instance);
        public int Count => Data.Count;

        IContextReference this[int i] => Data[i];
        public Size Size => Sizes.Size;
        public bool IsNone => Count == 0;
        IContextReference[] SortedData => SortedDataCache.Value;


        void AddRange(IEnumerable<IContextReference> a)
        {
            foreach(var e in a)
                Add(e);
        }

        void Add(IContextReference e)
        {
            if(!Data.Contains(e))
                Data.Add(e);
        }

        internal static Closures Void() => new Closures();
        internal static Closures Arg() => new Closures(CodeArg.Instance);

        public Closures Sequence(Closures closures)
            => closures.Count == 0? this :
                Count == 0? closures :
                new Closures(Data, closures.Data);

        internal static Closures Create(IContextReference contextReference)
            => new Closures(contextReference);

        protected override string GetNodeDump() => base.GetNodeDump() + "#" + Count;

        public override string DumpData()
        {
            var result = "";
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    result += "\n";
                result += Tracer.Dump(Data[i]);
            }

            return result;
        }

        SizeArray CalculateSizes()
        {
            var result = new SizeArray();
            for(var i = 0; i < Count; i++)
                result.Add(Data[i].Size());
            return result;
        }

        public Closures Without(IContextReference e)
        {
            if(!Data.Contains(e))
                return this;
            var r = new List<IContextReference>(Data);
            r.Remove(e);
            return new Closures(r);
        }

        IContextReference[] ObtainSortedData()
            => Data
                .OrderBy(codeArg => codeArg.Order)
                .ToArray();

        public Closures WithoutArg() => Without(CodeArg.Instance);

        Closures Without(Closures other)
            => other
                .Data
                .Aggregate(this, (current, refInCode) => current.Without(refInCode));

        public bool Contains(IContextReference context) => Data.Contains(context);

        public bool Contains(Closures other)
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

        public bool IsEqual(Closures other)
        {
            if(Count != other.Count)
                return false;

            for(var i = 0; i < Count; i++)
                if(SortedData[i].Order != other.SortedData[i].Order)
                    return false;

            return true;
        }

        internal CodeBase ToCode()
            => Data
                .Aggregate(CodeBase.Void, (current, t) => current + CodeBase.ReferenceCode(t));

        internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, CodeBase codeArgsReference)
        {
            var trace = ObjectId == -1;
            StartMethodDump(trace, code, codeArgsReference);
            try
            {
                var refSize = Root.DefaultRefAlignParam.RefSize;
                var reference = codeArgsReference.ReferencePlus(refSize * Data.Count);
                var result = code;
                foreach(var referenceInCode in Data)
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

        public static Closures operator +(Closures x, Closures y) => x.Sequence(y);
        public static Closures operator -(Closures x, Closures y) => x.Without(y);
        public static Closures operator -(Closures x, IContextReference y) => x.Without(y);
    }
}