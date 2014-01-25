#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Helper;
using hw.Forms;
using Reni.Basics;
using Reni.Code;

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
            StopByObjectId(-10);
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
        public List<IContextReference> Data { get { return _data; } }

        [DisableDump]
        SizeArray Sizes { get { return _sizesCache ?? (_sizesCache = CalculateSizes()); } }

        internal bool HasArg { get { return Contains(CodeArg.Instance); } }
        public int Count { get { return _data.Count; } }

        public IContextReference this[int i] { get { return _data[i]; } }
        public Size Size { get { return Sizes.Size; } }
        public bool IsNone { get { return Count == 0; } }
        public IContextReference[] SortedData { get { return _sortedDataCache.Value; } }

        internal static CodeArgs Void() { return new CodeArgs(); }
        internal static CodeArgs Arg() { return new CodeArgs(CodeArg.Instance); }

        public CodeArgs Sequence(CodeArgs codeArgs)
        {
            if(codeArgs.Count == 0)
                return this;
            if(Count == 0)
                return codeArgs;
            return new CodeArgs(_data, codeArgs._data);
        }

        internal static CodeArgs Create(IContextReference contextReference) { return new CodeArgs(contextReference); }

        protected override string GetNodeDump()
        {
            if(Count > 5)
                return base.GetNodeDump() + " Count = " + Count;
            return "{" + _data.Select(contextReference => contextReference.NodeDump()).Stringify(",") + "}";
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
                result.Add((this)[i].Size);
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

        IContextReference[] ObtainSortedData() { return _data.OrderBy(codeArg => codeArg.Order).ToArray(); }
        public CodeArgs WithoutArg() { return Without(CodeArg.Instance); }
        public CodeArgs Without(CodeArgs other) { return other._data.Aggregate(this, (current, refInCode) => current.Without(refInCode)); }
        public bool Contains(IContextReference context) { return _data.Contains(context); }
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
        {
            return _data
                .Aggregate(CodeBase.Void, (current, t) => current + CodeBase.ReferenceCode(t));
        }

        internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, Size refSize, CodeBase codeArgsReference)
        {
            var trace = ObjectId == -1;
            StartMethodDump(trace, code, refSize, codeArgsReference);
            try
            {
                var reference = codeArgsReference.ReferencePlus(refSize * _data.Count);
                var result = code;
                foreach(var referenceInCode in _data)
                {
                    Dump("reference", reference);
                    BreakExecution();
                    Tracer.Assert(referenceInCode.Size == refSize);
                    reference = reference.ReferencePlus(refSize * -1);
                    result = result.ReplaceAbsolute(referenceInCode, () => reference.DePointer(refSize));
                    Dump("result", result);
                }
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        public static CodeArgs operator +(CodeArgs x, CodeArgs y) { return x.Sequence(y); }
        public static CodeArgs operator -(CodeArgs x, CodeArgs y) { return x.Without(y); }
        public static CodeArgs operator -(CodeArgs x, IContextReference y) { return x.Without(y); }
        IEnumerable<TreeNode> ITreeNodeSupport.CreateNodes() { return _data.CreateNodes(); }

        sealed class CodeArg : Singleton<CodeArg, DumpableObject>, IContextReference
        {
            Size IContextReference.Size
            {
                get
                {
                    NotImplementedMethod();
                    return null;
                }
            }
            int IContextReference.Order { get { return -1; } }
            protected override string GetNodeDump() { return "CodeArg"; }
        }
    }
}