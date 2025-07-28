using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Type;

namespace Reni.Code;

abstract class CodeBase
    : DumpableObject, IIconKeyProvider, IFormalCodeItem, IAggregateable<CodeBase>
{
    [Node]
    [DisableDump]
    [field: AllowNull]
    [field: MaybeNull]
    internal Size Size => field ??= GetSize().AssertNotNull();

    [DisableDump]
    internal Size TemporarySize => GetTemporarySize();

    [DisableDump]
    [field: AllowNull]
    [field: MaybeNull]
    internal Closures Closures => field ??= GetClosures();


    [DisableDump]
    internal bool HasArguments => Visit(new HasArgumentVisitor());

    [DisableDump]
    internal TypeBase? ArgumentType => Visit(new ArgumentTypeVisitor());

    internal static CodeBase Void => BitArray.Void;

    [DisableDump]
    internal bool IsHollow => Size.IsZero;

    protected CodeBase(int objectId)
        : base(objectId)
        => StopByObjectIds();

    CodeBase IAggregateable<CodeBase>.Aggregate(CodeBase? other) => other == null? this : this + other;

    Size IFormalCodeItem.Size => Size;

    void IFormalCodeItem.Visit(IVisitor visitor) => Visit(visitor);

    /// <summary>
    ///     Gets the icon key.
    /// </summary>
    /// <value> The icon key. </value>
    string IIconKeyProvider.IconKey => "Code";

    protected abstract Size GetSize();

    internal abstract CodeBase Concat(FiberItem subsequentElement);

    [DisableDump]
    internal virtual bool IsEmpty => false;

    [DisableDump]
    internal virtual bool IsRelativeReference => false;

    protected virtual Size GetTemporarySize() => Size;

    protected virtual Closures GetClosures() => Closures.GetVoid();

    internal virtual IEnumerable<CodeBase> ToList() => [this];

    protected virtual TCode? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        => actual.Default(this);

    internal virtual void Visit(IVisitor visitor) => NotImplementedMethod(visitor);

    internal virtual CodeBase? ArrangeCleanupCode() => null;

    protected override string GetNodeDump() => base.GetNodeDump() + " Size=" + Size;

    internal CodeBase GetAssignment(Size size)
    {
        var alignedSize = size.ByteAlignedSize;
        return Concat(new Assign(alignedSize));
    }


    internal CodeBase GetThenElse(CodeBase thenCode, CodeBase elseCode)
        => Concat(new ThenElse(thenCode, elseCode));

    internal LocalReference GetLocalReference(TypeBase type, bool isUsedOnce = false) => new(type, this, isUsedOnce);

    internal CodeBase GetReferenceWithOffset(Size right)
    {
        if(right.IsZero)
            return this;
        return Concat(new ReferencePlusConstant(right, CallingMethodName));
    }

    internal CodeBase GetArrayGetter(Size elementSize, Size indexSize)
        => Concat(new ArrayGetter(elementSize, indexSize, CallingMethodName));

    internal CodeBase GetArraySetter(Size elementSize, Size indexSize)
        => Concat(new ArraySetter(elementSize, indexSize, CallingMethodName));

    internal CodeBase GetDePointer(Size targetSize)
    {
        if(Size.IsZero && targetSize.IsZero)
            return this;
        return Concat(new DePointer(targetSize.ByteAlignedSize, targetSize));
    }

    internal CodeBase GetBitCast(Size size)
    {
        if(Size == size)
            return this;
        return Concat(new BitCast(size, Size, Size));
    }

    CodeBase GetSequence(params CodeBase[] data) => this.Plus(data).GetCode();

    internal CodeBase ReplaceArgument(ResultCache argument)
    {
        try
        {
            var result = argument.Code.IsRelativeReference
                ? Visit(new ReplaceRelRefArgument(argument))
                : Visit(new ReplaceAbsoluteArgument(argument));
            return result ?? this;
        }
        catch(ReplaceArgument.TypeException typeException)
        {
            DumpDataWithBreak("", "this", this, "argument", argument, "typeException", typeException);
            throw;
        }
    }

    /// <summary>
    ///     Replaces appearances of context in code tree. Assumes, that replacement requires offset alignment when walking
    ///     along code tree
    /// </summary>
    /// <typeparam name="TContext"> </typeparam>
    /// <param name="context"> The context. </param>
    /// <param name="replacement"> The replacement. </param>
    /// <returns> </returns>
    internal CodeBase ReplaceRelative<TContext>(TContext context, Func<CodeBase> replacement)
        where TContext : IContextReference
    {
        var result = Visit(new ReplaceRelativeContextReference<TContext>(context, replacement));
        if(result != null)
            return result;
        return this;
    }

    /// <summary>
    ///     Replaces appearances of context in code tree. Assumes, that replacement isn't a reference, that changes when
    ///     walking along the code tree
    /// </summary>
    /// <typeparam name="TContext"> </typeparam>
    /// <param name="context"> The context. </param>
    /// <param name="replacement"> The replacement. </param>
    /// <returns> </returns>
    internal CodeBase ReplaceAbsolute<TContext>(TContext context, Func<CodeBase> replacement)
        where TContext : IContextReference
    {
        var result = Visit(new ReplaceAbsoluteContextReference<TContext>(context, replacement));
        if(result != null)
            return result;
        return this;
    }

    internal TCode? Visit<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        => VisitImplementation(actual);

    internal CodeBase GetCall(FunctionId index, Size resultSize)
    {
        var subsequentElement = new Call(index, resultSize, Size);
        return Concat(subsequentElement);
    }

    internal CodeBase GetForeignCall(MethodInfo methodInfo, Size resultSize)
    {
        var subsequentElement = new ForeignCall(methodInfo, resultSize, Size);
        return Concat(subsequentElement);
    }

    internal CodeBase TryReplacePrimitiveRecursivity(FunctionId functionId)
    {
        if(!Size.IsZero)
            return this;

        var newResult = Visit(new ReplacePrimitiveRecursivity(functionId));
        return newResult ?? this;
    }

    internal BitsConst GetValue(IExecutionContext context)
    {
        var dataStack = new DataStack(context);
        Visit(dataStack);
        return dataStack.Value;
    }

    internal CodeBase GetAlign()
        => GetBitCast(Size.GetNextPacketSize(Root.DefaultRefAlignParam.AlignBits));

    internal CodeBase GetLocalBlock(CodeBase copier)
        => new RemoveLocalReferences(this, copier).NewBody;

    internal CodeBase GetLocalBlockEnd(CodeBase copier, Size initialSize)
    {
        if(initialSize.IsZero)
            return this;

        copier.IsEmpty.Assert();
        return Concat(new Drop(Size, Size - initialSize));
    }

    internal CodeBase GetNumberOperation
        (string name, Size resultSize, Size leftSize, Size rightSize)
        => Concat(new BitArrayBinaryOp(name, resultSize, leftSize, rightSize));

    internal CodeBase GetDumpPrintNumber() => Concat(new DumpPrintNumberOperation(Size, Size.Zero));

    internal CodeBase GetDumpPrintNumber(Size leftSize)
        => Concat(new DumpPrintNumberOperation(leftSize, Size - leftSize));

    internal CodeBase GetDumpPrintText(Size itemSize)
        => Concat(new DumpPrintTextOperation(Size, itemSize.AssertNotNull()));

    internal CodeBase GetNumberOperation(string operation, Size size)
        => Concat(new BitArrayPrefixOp(operation, size, Size));

    internal CodeBase AddRange(IEnumerable<FiberItem> subsequentElement) => subsequentElement
        .Aggregate(this, (current, fiberItem) => current.Concat(fiberItem));

    internal void Execute(IExecutionContext context, ITraceCollector traceCollector)
    {
        try
        {
            Visit
            (
                new DataStack(context)
                {
                    TraceCollector = traceCollector
                });
        }
        catch(UnexpectedContextReference e)
        {
            Tracer.AssertionFailed("", () => e.Message);
        }
    }

    internal Container GetContainer(string description, FunctionId? functionId = null)
        => new(GetAlign(), null, description, functionId);

    public static CodeBase operator +(CodeBase a, CodeBase b) => a.GetSequence(b);

    internal CodeBase GetWithCleanupAdded(CodeBase cleanupCode) => new CodeWithCleanup(this, cleanupCode);

    internal CodeBase GetInvalidConversion(Size size) => Concat(new InvalidConversionCode(Size, size));
    internal static CodeBase GetFrameRef() => new TopFrameRef();
    internal static CodeBase GetTopRef() => new TopRef();
}

abstract class UnexpectedVisitOfPending : Exception;
