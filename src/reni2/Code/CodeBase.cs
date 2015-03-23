using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.ReniParser;
using Reni.Struct;
using Reni.Type;
using Reni.Validation;

namespace Reni.Code
{
    abstract class CodeBase
        : DumpableObject, IIconKeyProvider, IFormalCodeItem, IAggregateable<CodeBase>
    {
        protected CodeBase(int objectId)
            : base(objectId) {}

        [Node]
        [DisableDump]
        internal Size Size => GetSize();

        [DisableDump]
        internal Size TemporarySize => GetTemporarySize();

        [DisableDump]
        internal CodeArgs Exts => GetRefsImplementation();

        [DisableDump]
        internal virtual bool IsEmpty => false;

        [DisableDump]
        internal virtual bool IsRelativeReference => false;

        protected virtual Size GetTemporarySize() => Size;


        [DisableDump]
        internal bool HasArg => Visit(new HasArgVisitor());

        internal static CodeBase Issue(Issue issue) => new IssueCode(issue);
        internal static CodeBase BitsConst(Size size, BitsConst t) => new BitArray(size, t);
        internal static CodeBase BitsConst(BitsConst t) => BitsConst(t.Size, t);
        internal static CodeBase DumpPrintText(string dumpPrintText)
            => new DumpPrintText(dumpPrintText);
        internal static CodeBase FrameRef() => new TopFrameRef();
        internal static FiberItem RecursiveCall(Size refsSize)
            => new RecursiveCallCandidate(refsSize);
        internal static CodeBase ReferenceCode(IContextReference reference)
            => new ReferenceCode(reference);
        internal static CodeBase Void => BitArray.Void;
        internal static CodeBase TopRef() => new TopRef();

        internal static CodeBase List(IEnumerable<CodeBase> data)
        {
            var allData = data
                .SelectMany(item => item.AsList())
                .ToArray();

            var issues = IssueCode.CheckedCreate(allData.Where(item => item is IssueCode));
            var nonIssues = allData.Where(item => !(item is IssueCode));
            return Code.List.Create(issues.plus(nonIssues));
        }

        protected abstract Size GetSize();

        protected virtual CodeArgs GetRefsImplementation() => CodeArgs.Void();

        internal CodeBase Assignment(Size size)
        {
            var refAlignParam = Root.DefaultRefAlignParam;
            var alignedSize = size.ByteAlignedSize;
            return Add(new Assign(refAlignParam, alignedSize));
        }


        internal CodeBase ThenElse(CodeBase thenCode, CodeBase elseCode)
            => Add(new ThenElse(thenCode, elseCode));

        internal LocalReference LocalReference
            (TypeBase type, CodeBase destructorCode, bool isUsedOnce = false)
            => new LocalReference(type, this, destructorCode, isUsedOnce);

        internal abstract CodeBase Add(FiberItem subsequentElement);

        internal CodeBase ReferencePlus(Size right)
        {
            if(right.IsZero)
                return this;
            return Add(new ReferencePlusConstant(right, CallingMethodName));
        }

        internal CodeBase ArrayGetter(Size elementSize, Size indexSize)
            => Add(new ArrayGetter(elementSize, indexSize, CallingMethodName));
        internal CodeBase ArraySetter(Size elementSize, Size indexSize)
            => Add(new ArraySetter(elementSize, indexSize, CallingMethodName));

        internal CodeBase DePointer(Size targetSize)
        {
            if(Size.IsZero && targetSize.IsZero)
                return this;
            return Add(new DePointer(targetSize.ByteAlignedSize, targetSize));
        }

        internal CodeBase BitCast(Size size)
        {
            if(Size == size)
                return this;
            return Add(new BitCast(size, Size, Size));
        }

        CodeBase Sequence(params CodeBase[] data) => List(this.plus(data));

        protected virtual IEnumerable<CodeBase> AsList() => new[] {this};

        internal CodeBase ReplaceArg(TypeBase type, CodeBase code) => ReplaceArg
            (
                new Result
                {
                    Type = type,
                    Code = code
                });

        internal CodeBase ReplaceArg(ResultCache arg)
        {
            try
            {
                var result = arg.Code.IsRelativeReference
                    ? Visit(new ReplaceRelRefArg(arg))
                    : Visit(new ReplaceAbsoluteArg(arg));
                return result ?? this;
            }
            catch(ReplaceArg.TypeException typeException)
            {
                DumpDataWithBreak("", "this", this, "arg", arg, "typeException", typeException);
                throw;
            }
        }

        /// <summary>
        ///     Replaces appearences of context in code tree. Assumes, that replacement requires offset alignment when walking
        ///     along code tree
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
        ///     Replaces appearences of context in code tree. Assumes, that replacement isn't a reference, that changes when
        ///     walking along the code tree
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

        internal TResult Visit<TResult>(Visitor<TResult> actual) => VisitImplementation(actual);

        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual)
            => actual.Default(this);

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

        internal CodeBase Align()
            => BitCast(Size.NextPacketSize(Root.DefaultRefAlignParam.AlignBits));

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value> The icon key. </value>
        string IIconKeyProvider.IconKey => "Code";

        protected override string GetNodeDump() => base.GetNodeDump() + " Size=" + Size;

        [DisableDump]
        internal abstract IEnumerable<Issue> Issues { get; }

        [DisableDump]
        internal bool Hllw => Size.IsZero;

        internal CodeBase LocalBlock(CodeBase copier)
            => new RemoveLocalReferences(this, copier).NewBody;

        internal CodeBase LocalBlockEnd(CodeBase copier, Size initialSize)
        {
            if(initialSize.IsZero)
                return this;

            Tracer.Assert(copier.IsEmpty);
            return Add(new Drop(Size, Size - initialSize));
        }

        internal CodeBase NumberOperation
            (string name, Size resultSize, Size leftSize, Size rightSize)
            => Add(new BitArrayBinaryOp(name, resultSize, leftSize, rightSize));
        internal CodeBase DumpPrintNumber() => Add(new DumpPrintNumberOperation(Size, Size.Zero));
        internal CodeBase DumpPrintNumber(Size leftSize)
            => Add(new DumpPrintNumberOperation(leftSize, Size - leftSize));
        internal CodeBase DumpPrintText(Size itemSize)
            => Add(new DumpPrintTextOperation(Size, itemSize));
        internal CodeBase NumberOperation(string operation, Size size)
            => Add(new BitArrayPrefixOp(operation, size, Size));

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

        internal virtual void Visit(IVisitor visitor) => NotImplementedMethod(visitor);

        void IFormalCodeItem.Visit(IVisitor visitor) => Visit(visitor);

        protected static CodeArgs GetRefs(CodeBase[] codeBases)
        {
            var refs = codeBases.Select(code => code.Exts).ToArray();
            return refs.Aggregate(CodeArgs.Void(), (r1, r2) => r1.Sequence(r2));
        }

        internal static CodeBase Arg(TypeBase type) => new Arg(type);
        internal Container Container(string description, FunctionId functionId = null)
            => new Container(this, description, functionId);

        CodeBase IAggregateable<CodeBase>.Aggregate(CodeBase other) => this + other;

        public static CodeBase operator +(CodeBase a, CodeBase b) => a.Sequence(b);
    }

    abstract class UnexpectedVisitOfPending : Exception {}

    static class CodeBaseExtender
    {
        internal static CodeBase ToSequence(this IEnumerable<CodeBase> x)
        {
            return x.Aggregate(CodeBase.Void, (code, result) => code + result);
        }
    }
}