//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;

namespace Reni
{
    /// <summary>
    ///     Contains list of references to compiler environemnts.
    /// </summary>
    [Serializable]
    internal sealed class Refs : ReniObject, ITreeNodeSupport
    {
        private static int _nextId;
        private readonly List<IReferenceInCode> _data;
        private readonly bool _hasArg;
        private SizeArray _sizesCache;

        private Refs(bool hasArg)
            : base(_nextId++)
        {
            _hasArg = hasArg;
            _data = new List<IReferenceInCode>();
            StopByObjectId(-10);
        }

        private Refs(IReferenceInCode context)
            : this(false) { Add(context); }

        private Refs(bool hasArg, IEnumerable<IReferenceInCode> a, IEnumerable<IReferenceInCode> b)
            : this(hasArg)
        {
            AddRange(a);
            AddRange(b);
        }

        private Refs(bool hasArg, IEnumerable<IReferenceInCode> a)
            : this(hasArg) { AddRange(a); }


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

        [SmartNode]
        public List<IReferenceInCode> Data { get { return _data; } }
        internal override string DumpShort() { return base.DumpShort() + " HasArg=" + HasArg; }

        [DisableDump]
        private SizeArray Sizes { get { return _sizesCache ?? (_sizesCache = CalculateSizes()); } }

        internal bool HasArg { get { return _hasArg; } }
        public int Count { get { return _data.Count; } }

        public IReferenceInCode this[int i] { get { return _data[i]; } }
        public Size Size { get { return Sizes.Size; } }
        public bool IsNone { get { return Count == 0; } }

        internal static Refs ArgLess() { return new Refs(false); }
        internal static Refs Arg() { return new Refs(true); }

        public Refs Sequence(Refs refs)
        {
            if(refs.Count == 0 && !refs.HasArg)
                return this;
            if(Count == 0 && !HasArg)
                return refs;
            return new Refs(HasArg || refs.HasArg, _data, refs._data);
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
            return new Refs(HasArg, r);
        }

        public Refs Without(Refs other) { return other._data.Aggregate(this, (current, refInCode) => current.Without(refInCode)); }
        public bool Contains(IReferenceInCode context) { return _data.Contains(context); }
        public bool Contains(Refs other) { return other._data.All(Contains); }

        internal CodeBase ToCode()
        {
            return _data
                .Aggregate(CodeBase.Void(), (current, t) => current.Sequence(CodeBase.ReferenceCode(t)));
        }

        internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, RefAlignParam refAlignParam, CodeBase endOfRefsCode)
        {
            StartMethodDump(false, code, refAlignParam, endOfRefsCode);
            try
            {
                var reference = endOfRefsCode.AddToReference(refAlignParam, refAlignParam.RefSize*-_data.Count);
                var result = code;
                foreach(var referenceInCode in _data)
                {
                    Dump("reference", reference);
                    BreakExecution();
                    var unrefAlignment = referenceInCode.RefAlignParam;
                    Tracer.Assert(unrefAlignment.IsEqual(refAlignParam));
                    var unrefPtrAlignment = refAlignParam;
                    var replacement = reference.Dereference(unrefPtrAlignment, unrefAlignment.RefSize);
                    result = result.ReplaceAbsolute(referenceInCode, () => replacement);
                    Dump("result", result);
                    reference = reference.AddToReference(unrefPtrAlignment, unrefPtrAlignment.RefSize);
                }
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        public static Refs operator +(Refs x, Refs y) { return x.Sequence(y); }
        public static Refs operator -(Refs x, Refs y) { return x.Without(y); }
        public static Refs operator -(Refs x, IReferenceInCode y) { return x.Without(y); }
        TreeNode[] ITreeNodeSupport.CreateNodes() { return _data.CreateNodes(); }
    }
}