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
    sealed class CodeArgs : ReniObject, ITreeNodeSupport
    {
        static int _nextId;
        readonly List<IReferenceInCode> _data;
        SizeArray _sizesCache;

        CodeArgs()
            : base(_nextId++)
        {
            _data = new List<IReferenceInCode>();
            StopByObjectId(-10);
        }

        CodeArgs(IReferenceInCode context)
            : this() { Add(context); }

        CodeArgs(IEnumerable<IReferenceInCode> a, IEnumerable<IReferenceInCode> b)
            : this()
        {
            AddRange(a);
            AddRange(b);
        }

        CodeArgs(IEnumerable<IReferenceInCode> a)
            : this() { AddRange(a); }


        void AddRange(IEnumerable<IReferenceInCode> a)
        {
            foreach(var e in a)
                Add(e);
        }

        void Add(IReferenceInCode e)
        {
            if(!_data.Contains(e))
                _data.Add(e);
        }

        [SmartNode]
        public List<IReferenceInCode> Data { get { return _data; } }

        [DisableDump]
        SizeArray Sizes { get { return _sizesCache ?? (_sizesCache = CalculateSizes()); } }

        internal bool HasArg { get { return Contains(CodeArg.Instance); } }
        public int Count { get { return _data.Count; } }

        public IReferenceInCode this[int i] { get { return _data[i]; } }
        public Size Size { get { return Sizes.Size; } }
        public bool IsNone { get { return Count == 0; } }

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

        internal static CodeArgs Create(IReferenceInCode referenceInCode) { return new CodeArgs(referenceInCode); }

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
                result.Add((this)[i].RefAlignParam.RefSize);
            return result;
        }

        public CodeArgs Without(IReferenceInCode e)
        {
            if(!_data.Contains(e))
                return this;
            var r = new List<IReferenceInCode>(_data);
            r.Remove(e);
            return new CodeArgs(r);
        }

        public CodeArgs WithoutArg() { return Without(CodeArg.Instance); }
        public CodeArgs Without(CodeArgs other) { return other._data.Aggregate(this, (current, refInCode) => current.Without(refInCode)); }
        public bool Contains(IReferenceInCode context) { return _data.Contains(context); }
        public bool Contains(CodeArgs other) { return other._data.All(Contains); }

        internal CodeBase ToCode()
        {
            return _data
                .Aggregate(CodeBase.Void(), (current, t) => current.Sequence(CodeBase.ReferenceCode(t)));
        }

        internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, Size refSize, CodeBase codeArgsReference)
        {
            var trace = ObjectId == -1;
            StartMethodDump(trace, code, refSize, codeArgsReference);
            try
            {
                var reference = codeArgsReference.AddToReference(refSize * _data.Count);
                var result = code;
                foreach(var referenceInCode in _data)
                {
                    Dump("reference", reference);
                    BreakExecution();
                    Tracer.Assert(referenceInCode.RefSize == refSize);
                    reference = reference.AddToReference(refSize * -1);
                    result = result.ReplaceAbsolute(referenceInCode, () => reference.Dereference(refSize));
                    Dump("result", result);
                }
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        public static CodeArgs operator +(CodeArgs x, CodeArgs y) { return x.Sequence(y); }
        public static CodeArgs operator -(CodeArgs x, CodeArgs y) { return x.Without(y); }
        public static CodeArgs operator -(CodeArgs x, IReferenceInCode y) { return x.Without(y); }
        TreeNode[] ITreeNodeSupport.CreateNodes() { return _data.CreateNodes(); }

        internal sealed class CodeArg : ReniObject, IReferenceInCode
        {
            internal static readonly IReferenceInCode Instance = new CodeArg();
            RefAlignParam IReferenceInCode.RefAlignParam { get { return null; } }
            Size IReferenceInCode.RefSize { get {
                NotImplementedMethod();
                return null;
            } }
            string IDumpShortProvider.DumpShort() { return "CodeArg"; }
        }
    }
}