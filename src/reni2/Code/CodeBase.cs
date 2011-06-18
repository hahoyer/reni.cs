using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    [Serializable]
    internal abstract class CodeBase : ReniObject, IIconKeyProvider, IFormalCodeItem
    {
        protected CodeBase(int objectId)
            : base(objectId) { }

        protected CodeBase() { }

        [Node]
        [DisableDump]
        internal Size Size { get { return GetSize(); } }

        [Node]
        [DisableDump]
        internal Size MaxSize { get { return MaxSizeImplementation; } }

        [Node]
        [DisableDump]
        [SmartNode]
        internal List<IReferenceInCode> RefsData { get { return Refs.Data; } }

        [DisableDump]
        internal Refs Refs { get { return GetRefsImplementation(); } }

        [DisableDump]
        internal virtual bool IsEmpty { get { return false; } }

        [DisableDump]
        internal virtual bool IsRelativeReference { get { return RefAlignParam != null; } }

        [DisableDump]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        [DisableDump]
        protected virtual Size MaxSizeImplementation { get { return Size; } }

        protected abstract Size GetSize();

        protected virtual Refs GetRefsImplementation() { return Refs.None(); }

        internal CodeBase CreateAssignment(RefAlignParam refAlignParam, Size size)
        {
            var alignedSize = size.ByteAlignedSize;
            return CreateFiber(new Assign(refAlignParam, alignedSize));
        }

        internal static CodeBase DumpPrintText(string dumpPrintText) { return new DumpPrintText(dumpPrintText); }

        internal CodeBase CreateThenElse(CodeBase thenCode, CodeBase elseCode) { return CreateFiber(new ThenElse(thenCode, elseCode)); }

        internal static CodeBase TopRef(RefAlignParam refAlignParam) { return new TopRef(refAlignParam, CallingMethodName); }

        private static string CallingMethodName
        {
            get
            {
                if(Debugger.IsAttached)
                    return Tracer.CallingMethodName(2);
                return "";
            }
        }

        internal LocalReference LocalReference(RefAlignParam refAlignParam, CodeBase destructorCode) { return new LocalReference(refAlignParam, this, destructorCode); }

        internal static CodeBase FrameRef(RefAlignParam refAlignParam) { return new TopFrameRef(refAlignParam, Size.Create(0), CallingMethodName); }

        internal abstract CodeBase CreateFiber(FiberItem subsequentElement);

        internal CodeBase AddToReference(RefAlignParam refAlignParam, Size right, string reason = "")
        {
            if(right.IsZero)
                return this;
            if (reason != "")
                reason = "(" + reason + ")";
            return CreateFiber(new RefPlus(refAlignParam, right, CallingMethodName + reason));
        }

        internal CodeBase Dereference(RefAlignParam refAlignParam, Size targetSize)
        {
            Tracer.Assert(Size == refAlignParam.RefSize);
            return CreateFiber(new Dereference(refAlignParam, targetSize, targetSize));
        }

        internal CodeBase CreateBitCast(Size size)
        {
            if(Size == size)
                return this;
            return CreateFiber(new BitCast(size, Size, Size));
        }

        internal virtual CodeBase Sequence(params CodeBase[] data)
        {
            var extendedData = new CodeBase[data.Length + 1];
            extendedData[0] = this;
            data.CopyTo(extendedData, 1);
            return List(extendedData);
        }

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

        protected virtual IEnumerable<CodeBase> AsList() { return new[] {this}; }

        internal static CodeBase BitsConst(Size size, BitsConst t) { return new BitArray(size, t); }
        internal static CodeBase BitsConst(BitsConst t) { return BitsConst(t.Size, t); }
        internal static CodeBase Void() { return BitArray.Void(); }
        internal static CodeBase Arg(Size size) { return new Arg(size); }
        internal static CodeBase ReferenceInCode(IReferenceInCode reference) { return new ReferenceCode(reference); }

        internal CodeBase ReplaceArg(CodeBase argCode)
        {
            try
            {
                var result = argCode.IsRelativeReference
                                 ? Visit(new ReplaceRelRefArg(argCode))
                                 : Visit(new ReplaceAbsoluteArg(argCode));
                return result ?? this;
            }
            catch(ReplaceArg.SizeException sizeException)
            {
                DumpWithBreak(true, "this", this, "sizeException", sizeException);
                throw;
            }
        }

        [DisableDump]
        internal bool HasArg { get { return Visit(new HasArgVisitor()); } }

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

        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Default(); }

        internal CodeBase CreateCall(int index, Size resultSize) { return CreateFiber(new Call(index, resultSize, Size)); }

        internal static FiberItem CreateRecursiveCall(Size refsSize) { return new RecursiveCallCandidate(refsSize); }

        internal CodeBase TryReplacePrimitiveRecursivity(int functionIndex)
        {
            if(!Size.IsZero)
                return this;

            var newResult = Visit(new ReplacePrimitiveRecursivity(functionIndex));
            return newResult ?? this;
        }

        internal virtual BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }

        internal CodeBase Align(int alignBits = Reni.BitsConst.SegmentAlignBits) { return CreateBitCast(Size.NextPacketSize(alignBits)); }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Code"; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Size=" + Size; } }

        internal CodeBase LocalBlock(CodeBase copier, RefAlignParam refAlignParam)
        {
            return new LocalReferenceSequenceVisitor()
                .LocalBlock(this, copier, refAlignParam);
        }

        internal CodeBase LocalBlockEnd(CodeBase copier, RefAlignParam refAlignParam, Size resultSize, string holder)
        {
            var intermediateSize = Size - resultSize;
            if(intermediateSize.IsZero)
                return this;

            var result = this;
            if(!resultSize.IsZero)
            {
                result = result.CreateFiber(new LocalBlockEnd(resultSize, intermediateSize))
                    .Sequence(copier.ReplaceArg(LocalVariableReference(refAlignParam, holder)));
            }

            return result.CreateFiber(new Drop(Size, resultSize));
        }

        internal static CodeBase BitSequenceOperation
            (Size size, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            return TypeBase.Bit
                .Sequence((objSize.ByteAlignedSize).ToInt())
                .ArgCode()
                .BitSequenceOperation(feature, size);
        }

        internal static CodeBase BitSequenceOperation
            (Size size, ISequenceOfBitBinaryOperation token, int objBits, int argsBits)
        {
            var objSize = Size.Create(objBits);
            var argsSize = Size.Create(argsBits);
            return TypeBase.Bit.Sequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .ArgCode()
                .BitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        internal static CodeBase BitSequenceDumpPrint(int objSize)
        {
            var alignedSize = Size.Create(objSize).ByteAlignedSize;
            return TypeBase.Bit.Sequence(alignedSize.ToInt())
                .ArgCode()
                .DumpPrint(alignedSize);
        }

        private CodeBase BitSequenceOperation(ISequenceOfBitBinaryOperation name, Size size, Size leftSize) { return CreateFiber(new BitArrayBinaryOp(name, size, leftSize, Size - leftSize)); }
        private CodeBase DumpPrint(Size leftSize) { return CreateFiber(new DumpPrintOperation(leftSize, Size - leftSize)); }
        private CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation feature, Size size) { return CreateFiber(new BitArrayPrefixOp(feature, size, Size)); }

        internal static CodeBase LocalVariableReference(RefAlignParam refAlignParam, string holder, Size offset = null) { return new LocalVariableReference(refAlignParam, holder, offset); }

        internal CodeBase CreateFiber(IEnumerable<FiberItem> subsequentElement)
        {
            return subsequentElement
                .Aggregate(this, (current, fiberItem) => current.CreateFiber(fiberItem));
        }

        protected virtual string CSharpString()
        {
            NotImplementedMethod();
            return "";
        }

        internal virtual CSharpCodeSnippet CSharpCodeSnippet() { return new CSharpCodeSnippet("", CSharpString()); }

        internal virtual string ReversePolish(Size top) { return CSharpString(top); }

        protected virtual string CSharpString(Size top)
        {
            NotImplementedMethod(top);
            return "";
        }

        internal void Execute(CodeBase[] functions, bool isTraceEnabled)
        {
            try
            {
                Execute(new DataStack(functions, isTraceEnabled));
            }
            catch(UnexpectedContextReference e)
            {
                Tracer.AssertionFailed("", () => e.Message);
            }
        }

        protected virtual void Execute(IFormalMaschine formalMaschine) { NotImplementedMethod(formalMaschine); }

        void IFormalCodeItem.Execute(IFormalMaschine formalMaschine) { Execute(formalMaschine); }

        protected static Refs GetRefs(CodeBase[] codeBases)
        {
            var refs = codeBases.Select(code => code.Refs).ToArray();
            return refs.Aggregate(Refs.None(), (r1, r2) => r1.Sequence(r2));
        }
    }

    internal abstract class UnexpectedVisitOfPending : Exception
    {}

    internal static class CodeBaseExtender
    {
        internal static CodeBase ToSequence(this IEnumerable<CodeBase> x) { return x.Aggregate(CodeBase.Void(), (code, result) => code.Sequence(result)); }

        internal static CodeBase ToLocalVariables(this IEnumerable<CodeBase> codeBases, string holderPattern) { return CodeBase.List(codeBases.Select((x, i) => LocalVariableDefinition(string.Format(holderPattern, i), x))); }

        private static CodeBase LocalVariableDefinition(string holderName, CodeBase value) { return value.CreateFiber(new LocalVariableDefinition(holderName, value.Size)); }
    }
}