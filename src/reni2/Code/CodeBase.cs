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
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    [Serializable]
    abstract class CodeBase : ReniObject, IIconKeyProvider, IFormalCodeItem
    {
        static string _newCombinedReason;
        readonly string _reason;

        [DisableDump]
        internal string ReasonForCombine { get { return _reason == "" ? DumpShortForDebug() : _reason; } }

        [DisableDump]
        internal string NewCombinedReason
        {
            private get
            {
                if(_newCombinedReason == null)
                    return "";
                var result = _newCombinedReason;
                _newCombinedReason = null;
                return result;
            }
            set
            {
                Tracer.Assert(_newCombinedReason == null);
                _newCombinedReason = value;
            }
        }

        [EnableDumpExcept("")]
        internal string Reason { get { return _reason; } }

        protected CodeBase(int objectId)
            : base(objectId) { _reason = NewCombinedReason; }

        [Node]
        [DisableDump]
        internal Size Size { get { return GetSize(); } }

        [DisableDump]
        internal Size TemporarySize { get { return GetTemporarySize(); } }

        [DisableDump]
        internal CodeArgs CodeArgs { get { return GetRefsImplementation(); } }

        [DisableDump]
        internal virtual bool IsEmpty { get { return false; } }

        [DisableDump]
        internal virtual bool IsRelativeReference { get { return RefAlignParam != null; } }

        [DisableDump]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        protected virtual Size GetTemporarySize() { return Size; }

        [DisableDump]
        internal bool HasArg { get { return Visit(new HasArgVisitor()); } }

        internal static CodeBase BitsConst(Size size, BitsConst t) { return new BitArray(size, t); }
        internal static CodeBase BitsConst(BitsConst t) { return BitsConst(t.Size, t); }
        internal static CodeBase DumpPrintText(string dumpPrintText) { return new DumpPrintText(dumpPrintText); }
        internal static CodeBase FrameRef(RefAlignParam refAlignParam) { return new TopFrameRef(refAlignParam); }
        internal static FiberItem RecursiveCall(Size refsSize) { return new RecursiveCallCandidate(refsSize); }
        internal static CodeBase ReferenceCode(IReferenceInCode reference) { return new ReferenceCode(reference); }
        internal static CodeBase Void() { return BitArray.Void(); }
        internal static CodeBase TopRef(RefAlignParam refAlignParam) { return new TopRef(refAlignParam); }

        internal static CodeBase List(IEnumerable<CodeBase> data)
        {
            var resultData = new List<CodeBase>();
            foreach(var codeBase in data)
                resultData.AddRange(codeBase.AsList());
            switch(resultData.Count)
            {
                case 0:
                    return Void();
                case 1:
                    return resultData[0];
            }
            return Code.List.Create(resultData);
        }

        protected abstract Size GetSize();

        protected virtual CodeArgs GetRefsImplementation() { return CodeArgs.Void(); }

        internal CodeBase Assignment(RefAlignParam refAlignParam, Size size)
        {
            var alignedSize = size.ByteAlignedSize;
            return CreateFiber(new Assign(refAlignParam, alignedSize));
        }


        internal CodeBase ThenElse(CodeBase thenCode, CodeBase elseCode) { return CreateFiber(new ThenElse(thenCode, elseCode)); }

        internal LocalReference LocalReference(RefAlignParam refAlignParam, CodeBase destructorCode) { return new LocalReference(refAlignParam, this, destructorCode); }

        internal abstract CodeBase CreateFiber(FiberItem subsequentElement);

        internal CodeBase AddToReference(RefAlignParam refAlignParam, Size right)
        {
            if(right.IsZero)
                return this;
            return CreateFiber(new RefPlus(refAlignParam, right, CallingMethodName));
        }

        internal CodeBase Dereference(RefAlignParam refAlignParam, Size targetSize)
        {
            if(Size.IsZero && targetSize.IsZero)
                return this;
            Tracer.Assert(Size == refAlignParam.RefSize);
            return CreateFiber(new Dereference(refAlignParam, targetSize, targetSize));
        }

        internal CodeBase BitCast(Size size)
        {
            if(Size == size)
                return this;
            return CreateFiber(new BitCast(size, Size, Size));
        }

        internal CodeBase Sequence(params CodeBase[] data)
        {
            var extendedData = new CodeBase[data.Length + 1];
            extendedData[0] = this;
            data.CopyTo(extendedData, 1);
            return List(extendedData);
        }

        protected virtual IEnumerable<CodeBase> AsList() { return new[] {this}; }

        internal CodeBase ReplaceArg(TypeBase type, CodeBase code) { return ReplaceArg(new Result {Type = type, Code = code}); }
        internal CodeBase ReplaceArg(Result arg)
        {
            try
            {
                var result = arg.Code.IsRelativeReference
                                 ? Visit(new ReplaceRelRefArg(arg))
                                 : Visit(new ReplaceAbsoluteArg(arg));
                return result ?? this;
            }
            catch(ReplaceArg.SizeException sizeException)
            {
                DumpDataWithBreak("", "this", this, "arg", arg, "sizeException", sizeException);
                throw;
            }
        }

        /// <summary>
        ///     Replaces appearences of context in code tree.                                                               
        ///     Assumes, that replacement requires offset alignment when walking along code tree
        /// </summary>
        /// <typeparam name = "TContext"></typeparam>
        /// <param name = "context">The context.</param>
        /// <param name = "replacement">The replacement.</param>
        /// <returns></returns>
        internal CodeBase ReplaceRelative<TContext>(TContext context, Func<CodeBase> replacement)
            where TContext : IReferenceInCode
        {
            var result = Visit(new ReplaceRelativeContextRef<TContext>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        /// <summary>
        ///     Replaces appearences of context in code tree. 
        ///     Assumes, that replacement isn't a reference, that changes when walking along the code tree
        /// </summary>
        /// <typeparam name = "TContext"></typeparam>
        /// <param name = "context">The context.</param>
        /// <param name = "replacement">The replacement.</param>
        /// <returns></returns>
        internal CodeBase ReplaceAbsolute<TContext>(TContext context, Func<CodeBase> replacement)
            where TContext : IReferenceInCode
        {
            var result = Visit(new ReplaceAbsoluteContextRef<TContext>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        internal TResult Visit<TResult>(Visitor<TResult> actual) { return VisitImplementation(actual); }

        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Default(this); }

        internal CodeBase Call(int index, Size resultSize) { return CreateFiber(new Call(index, resultSize, Size)); }

        internal CodeBase TryReplacePrimitiveRecursivity(int functionIndex)
        {
            if(!Size.IsZero)
                return this;

            var newResult = Visit(new ReplacePrimitiveRecursivity(functionIndex));
            return newResult ?? this;
        }

        internal BitsConst Evaluate(IOutStream outStream)
        {
            var dataStack = new DataStack(new CodeBase[0], false, outStream);
            Visit(dataStack);
            return dataStack.Value;
        }

        internal CodeBase Align(int alignBits = Basics.BitsConst.SegmentAlignBits) { return BitCast(Size.NextPacketSize(alignBits)); }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Code"; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Size=" + Size; } }

        internal CodeBase LocalBlock(CodeBase copier)
        {
            return new LocalReferenceSequenceVisitor()
                .LocalBlock(this, copier);
        }

        internal CodeBase LocalBlockEnd(CodeBase copier, Size resultSize)
        {
            var intermediateSize = Size - resultSize;
            if(intermediateSize.IsZero)
                return this;

            Tracer.Assert(copier.IsEmpty);

            var result = this;
            if(!resultSize.IsZero)
                result = result.CreateFiber(new LocalBlockEnd(resultSize, intermediateSize));

            return result.CreateFiber(new Drop(Size, resultSize));
        }

        internal CodeBase BitSequenceOperation(ISequenceOfBitBinaryOperation name, Size size, Size leftSize) { return CreateFiber(new BitArrayBinaryOp(name, size, leftSize, Size - leftSize)); }
        internal CodeBase DumpPrintNumber(Size leftSize) { return CreateFiber(new DumpPrintNumberOperation(leftSize, Size - leftSize)); }
        internal CodeBase DumpPrintText(Size itemSize) { return CreateFiber(new DumpPrintTextOperation(Size, itemSize)); }
        internal CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation feature, Size size) { return CreateFiber(new BitArrayPrefixOp(feature, size, Size)); }

        internal static CodeBase LocalVariableReference(RefAlignParam refAlignParam, string holder, Size offset = null) { return new LocalVariableReference(refAlignParam, holder, offset); }

        internal CodeBase CreateFiber(IEnumerable<FiberItem> subsequentElement)
        {
            return subsequentElement
                .Aggregate(this, (current, fiberItem) => current.CreateFiber(fiberItem));
        }

        internal void Execute(CodeBase[] functions, bool isTraceEnabled, IOutStream outStream)
        {
            try
            {
                Visit(new DataStack(functions, isTraceEnabled, outStream));
            }
            catch(UnexpectedContextReference e)
            {
                Tracer.AssertionFailed("", () => e.Message);
            }
        }

        internal virtual void Visit(IVisitor visitor) { NotImplementedMethod(visitor); }

        void IFormalCodeItem.Visit(IVisitor visitor) { Visit(visitor); }

        protected static CodeArgs GetRefs(CodeBase[] codeBases)
        {
            var refs = codeBases.Select(code => code.CodeArgs).ToArray();
            return refs.Aggregate(CodeArgs.Void(), (r1, r2) => r1.Sequence(r2));
        }

        internal static CodeBase Arg(TypeBase type) { return new Arg(type); }
    }

    abstract class UnexpectedVisitOfPending : Exception
    {}

    static class CodeBaseExtender
    {
        internal static CodeBase ToSequence(this IEnumerable<CodeBase> x) { return x.Aggregate(CodeBase.Void(), (code, result) => code.Sequence(result)); }

        internal static CodeBase ToLocalVariables(this IEnumerable<CodeBase> codeBases, string holderPattern) { return CodeBase.List(codeBases.Select((x, i) => LocalVariableDefinition(string.Format(holderPattern, i), x))); }

        static CodeBase LocalVariableDefinition(string holderName, CodeBase value) { return value.CreateFiber(new LocalVariableDefinition(holderName, value.Size)); }
    }
}