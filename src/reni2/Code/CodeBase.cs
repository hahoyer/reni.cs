#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Struct;
using Reni.Type;
using Reni.Validation;

namespace Reni.Code
{
    [Serializable]
    abstract class CodeBase : ReniObject, IIconKeyProvider, IFormalCodeItem
    {
        protected CodeBase(int objectId)
            : base(objectId) { }

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
        internal virtual bool IsRelativeReference { get { return false; } }

        protected virtual Size GetTemporarySize() { return Size; }

        [DisableDump]
        internal bool HasArg { get { return Visit(new HasArgVisitor()); } }

        internal static CodeBase Issue(IssueBase issue) { return new IssueCode(issue); }
        internal static CodeBase BitsConst(Size size, BitsConst t) { return new BitArray(size, t); }
        internal static CodeBase BitsConst(BitsConst t) { return BitsConst(t.Size, t); }
        internal static CodeBase DumpPrintText(string dumpPrintText) { return new DumpPrintText(dumpPrintText); }
        internal static CodeBase FrameRef(RefAlignParam refAlignParam) { return new TopFrameRef(); }
        internal static FiberItem RecursiveCall(Size refsSize) { return new RecursiveCallCandidate(refsSize); }
        internal static CodeBase ReferenceCode(IContextReference reference) { return new ReferenceCode(reference); }
        internal static CodeBase Void { get { return BitArray.Void; } }
        internal static CodeBase TopRef(RefAlignParam refAlignParam) { return new TopRef(); }

        internal static CodeBase List(IEnumerable<CodeBase> data)
        {
            var resultData = new List<CodeBase>();
            foreach(var codeBase in data)
                resultData.AddRange(codeBase.AsList());
            switch(resultData.Count)
            {
                case 0:
                    return Void;
                case 1:
                    return resultData[0];
            }
            return Code.List.Create(resultData);
        }

        protected abstract Size GetSize();

        protected virtual CodeArgs GetRefsImplementation() { return CodeArgs.Void(); }

        internal CodeBase Assignment(Size size)
        {
            var refAlignParam = Root.DefaultRefAlignParam;
            var alignedSize = size.ByteAlignedSize;
            return Add(new Assign(refAlignParam, alignedSize));
        }


        internal CodeBase ThenElse(CodeBase thenCode, CodeBase elseCode) { return Add(new ThenElse(thenCode, elseCode)); }

        internal LocalReference LocalReference(CodeBase destructorCode) { return new LocalReference(this, destructorCode); }

        internal abstract CodeBase Add(FiberItem subsequentElement);

        internal CodeBase ReferencePlus(Size right)
        {
            if(right.IsZero)
                return this;
            return Add(new ReferencePlusConstant(right, CallingMethodName));
        }

        internal CodeBase ArrayAccess(Size elementSize, Size indexSize) { return Add(new ArrayGetter(elementSize, indexSize, CallingMethodName)); }
        internal CodeBase ArrayAssignment(Size elementSize, Size indexSize) { return Add(new ArraySetter(elementSize, indexSize, CallingMethodName)); }

        internal CodeBase Dereference(Size targetSize)
        {
            if(Size.IsZero && targetSize.IsZero)
                return this;
            return Add(new Dereference(targetSize, targetSize));
        }

        internal CodeBase BitCast(Size size)
        {
            if(Size == size)
                return this;
            return Add(new BitCast(size, Size, Size));
        }

        CodeBase Sequence(params CodeBase[] data)
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
        ///     Replaces appearences of context in code tree. Assumes, that replacement requires offset alignment when walking along code tree
        /// </summary>
        /// <typeparam name="TContext"> </typeparam>
        /// <param name="context"> The context. </param>
        /// <param name="replacement"> The replacement. </param>
        /// <returns> </returns>
        internal CodeBase ReplaceRelative<TContext>(TContext context, Func<CodeBase> replacement)
            where TContext : IContextReference
        {
            var result = Visit(new ReplaceRelativeContextRef<TContext>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        /// <summary>
        ///     Replaces appearences of context in code tree. Assumes, that replacement isn't a reference, that changes when walking along the code tree
        /// </summary>
        /// <typeparam name="TContext"> </typeparam>
        /// <param name="context"> The context. </param>
        /// <param name="replacement"> The replacement. </param>
        /// <returns> </returns>
        internal CodeBase ReplaceAbsolute<TContext>(TContext context, Func<CodeBase> replacement)
            where TContext : IContextReference
        {
            var result = Visit(new ReplaceAbsoluteContextRef<TContext>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        internal TResult Visit<TResult>(Visitor<TResult> actual) { return VisitImplementation(actual); }

        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Default(this); }

        internal CodeBase Call(FunctionId index, Size resultSize)
        {
            var subsequentElement = new Call(index, resultSize, Size);
            return Add(subsequentElement);
        }

        internal CodeBase TryReplacePrimitiveRecursivity(FunctionId functionId)
        {
            if(!Size.IsZero)
                return this;

            var newResult = Visit(new ReplacePrimitiveRecursivity(functionId));
            return newResult ?? this;
        }

        internal BitsConst Evaluate(IExecutionContext context)
        {
            var dataStack = new DataStack(context);
            Visit(dataStack);
            return dataStack.Value;
        }

        internal CodeBase Align() { return BitCast(Size.NextPacketSize(Root.DefaultRefAlignParam.AlignBits)); }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value> The icon key. </value>
        string IIconKeyProvider.IconKey { get { return "Code"; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + " Size=" + Size; }

        [DisableDump]
        internal abstract IEnumerable<IssueBase> Issues { get; }

        [DisableDump]
        internal bool IsDataLess { get { return Size.IsZero; } }

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
                result = result.Add(new LocalBlockEnd(resultSize, intermediateSize));

            return result.Add(new Drop(Size, resultSize));
        }

        internal CodeBase BitSequenceOperation(string name, Size size, Size leftSize) { return Add(new BitArrayBinaryOp(name, size, leftSize, Size - leftSize)); }
        internal CodeBase DumpPrintNumber(Size leftSize) { return Add(new DumpPrintNumberOperation(leftSize, Size - leftSize)); }
        internal CodeBase DumpPrintText(Size itemSize) { return Add(new DumpPrintTextOperation(Size, itemSize)); }
        internal CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation feature, Size size) { return Add(new BitArrayPrefixOp(feature, size, Size)); }

        internal static CodeBase LocalVariableReference(string holder, Size offset = null) { return new LocalVariableReference(holder, offset); }

        internal CodeBase AddRange(IEnumerable<FiberItem> subsequentElement)
        {
            return subsequentElement
                .Aggregate(this, (current, fiberItem) => current.Add(fiberItem));
        }

        internal void Execute(IExecutionContext context)
        {
            try
            {
                Visit(new DataStack(context));
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
        internal Container Container(string description, FunctionId functionId = null) { return new Container(this, description, functionId); }

        public static CodeBase operator +(CodeBase a, CodeBase b) { return a.Sequence(b); }
    }

    abstract class UnexpectedVisitOfPending : Exception
    {}

    static class CodeBaseExtender
    {
        internal static CodeBase ToSequence(this IEnumerable<CodeBase> x) { return x.Aggregate(CodeBase.Void, (code, result) => code + result); }

        internal static CodeBase ToLocalVariables(this IEnumerable<CodeBase> codeBases, string holderPattern) { return CodeBase.List(codeBases.Select((x, i) => LocalVariableDefinition(string.Format(holderPattern, i), x))); }

        static CodeBase LocalVariableDefinition(string holderName, CodeBase value) { return value.Add(new LocalVariableDefinition(holderName, value.Size)); }
    }
}