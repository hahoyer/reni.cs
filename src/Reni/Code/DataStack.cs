using Reni.Basics;
using Reni.Context;
using Reni.Struct;

namespace Reni.Code;

sealed class DataStack : DumpableObject, IVisitor
{
    internal sealed class DataMemento
    {
        internal readonly string ValueDump;
        internal int Offset;
        internal int Size;
        internal DataMemento(string valueDump) => ValueDump = valueDump;
    }

    sealed class LocalDataClass : DumpableObject, IStackDataAddressBase
    {
        public StackData Data;
        public FrameData Frame = new(null);
        public LocalDataClass(IOutStream? outStream) => Data = new EmptyStackData(outStream);

        string IStackDataAddressBase.Dump() => "stack";

        StackData IStackDataAddressBase.GetTop(Size offset, Size size)
            => Data.DoPull(Data.Size + offset).DoGetTop(size);

        void IStackDataAddressBase.SetTop(Size offset, StackData right)
        {
            var oldTop = Data.DoGetTop(Data.Size + offset);
            var tail = Data.DoPull(Data.Size + offset + right.Size);
            var newDataTail = tail.Push(right);
            var newData = newDataTail.Push(oldTop);
            Data = newData;
        }

        internal StackDataAddress Address(Size offset) => new(this, offset - Data.Size, Data.OutStream);

        internal StackData FrameAddress(Size offset)
            => new StackDataAddress(Frame, offset, Data.OutStream);
    }

    [DisableDump]
    internal ITraceCollector? TraceCollector;

    readonly IExecutionContext Context;

    [EnableDump]
    readonly LocalDataClass LocalData;

    internal static Size RefSize => Root.DefaultRefAlignParam.RefSize;

    [DisableDump]
    internal BitsConst Value => Data.GetBitsConst();

    StackData Data
    {
        get => LocalData.Data;
        set => LocalData.Data = value;
    }

    internal Size Size
    {
        get => Data.Size;
        set
        {
            if(Size == value)
                return;
            if(Size < value)
                Push(new UnknownStackData(value - Size, Data.OutStream));
            else
                Data = Data.ForcedPull(Size - value);
        }
    }

    public DataStack(IExecutionContext context)
    {
        Context = context;
        LocalData = new(Context.OutStream);
    }

    void IVisitor.ArrayGetter(Size elementSize, Size indexSize)
    {
        var offset = elementSize * Pull(indexSize).GetBitsConst().ToInt32();
        var baseAddress = Pull(RefSize);
        Push(baseAddress.RefPlus(offset));
    }

    void IVisitor.ArraySetter(Size elementSize, Size indexSize)
    {
        var right = Pull(RefSize);
        var offset = elementSize * Pull(indexSize).GetBitsConst().ToInt32();
        var baseAddress = Pull(RefSize);
        var left = baseAddress.RefPlus(offset);
        left.Assign(elementSize, right);
    }

    void IVisitor.Assign(Size targetSize)
    {
        var right = Pull(RefSize);
        var left = Pull(RefSize);
        left.Assign(targetSize, right);
    }

    void IVisitor.BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
    {
        var right = Pull(rightSize);
        var left = Pull(leftSize);
        Push(left.BitArrayBinaryOp(opToken, size, right));
    }

    void IVisitor.BitArrayPrefixOp(string operation, Size size, Size argSize)
    {
        var arg = Pull(argSize);
        Push(arg.BitArrayPrefixOp(operation, size));
    }

    void IVisitor.BitCast(Size size, Size targetSize, Size significantSize)
    {
        TracerAssert
        (
            size == targetSize,
            () =>
                nameof(size)
                + " == "
                + nameof(targetSize)
                + " "
                + nameof(size)
                + "="
                + size
                + " "
                + nameof(targetSize)
                + "="
                + targetSize);
        Push(Pull(targetSize).BitCast(significantSize).BitCast(size));
    }

    void IVisitor.BitsArray(Size size, BitsConst data)
    {
        if(size.IsZero)
            return;
        Push(new BitsStackData(data.Resize(size), Context.OutStream));
    }

    void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
    {
        var oldFrame = LocalData.Frame;
        var argsAndRefs = Pull(argsAndRefsSize);
        TraceCollector?.Call(argsAndRefs, functionId);
        do
        {
            LocalData.Frame = new(argsAndRefs);
            SubVisit(Context.Function(functionId));
        }
        while(LocalData.Frame.IsRepeatRequired);

        TraceCollector?.Return();
        LocalData.Frame = oldFrame;
    }

    void IVisitor.DePointer(Size size, Size dataSize)
    {
        var value = Pull(RefSize);
        Push(value.Dereference(dataSize, dataSize).BitCast(size));
    }

    void IVisitor.Drop(Size beforeSize, Size afterSize)
    {
        var top = Data.DoGetTop(afterSize);
        Pull(beforeSize);
        Push(top);
    }

    void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
    {
        SubVisit(fiberHead);
        foreach(var codeBase in fiberItems)
            SubVisit(codeBase);
    }

    void IVisitor.List(CodeBase[] data)
    {
        foreach(var codeBase in data)
            SubVisit(codeBase);
    }

    void IVisitor.PrintNumber(Size leftSize, Size rightSize)
    {
        TracerAssert(rightSize.IsZero, () => "rightSize.IsZero");
        Pull(leftSize).PrintNumber();
    }

    void IVisitor.PrintText(Size size, Size itemSize) => Pull(size).PrintText(itemSize);

    void IVisitor.PrintText(string dumpPrintText) => Context.OutStream?.AddData(dumpPrintText);

    void IVisitor.RecursiveCall() => LocalData.Frame.IsRepeatRequired = true;

    void IVisitor.RecursiveCallCandidate() => throw new NotImplementedException();

    void IVisitor.ReferencePlus(Size right) => Push(Pull(RefSize).RefPlus(right));

    void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
    {
        var bitsConst = Pull(condSize).GetBitsConst();
        SubVisit(bitsConst.IsZero? elseCode : thenCode);
    }

    void IVisitor.TopData(Size offset, Size size, Size dataSize)
    {
        var value = Data
            .DoPull(offset)
            .DoGetTop(dataSize)
            .BitCast(size);
        Push(value);
    }

    void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
    {
        var frame = LocalData.Frame.Data;
        var value = frame
            .DoPull(offset)
            .DoGetTop(size)
            .BitCast(dataSize)
            .BitCast(size);
        Push(value);
    }

    void IVisitor.TopFrameRef(Size offset) => Push(LocalData.FrameAddress(offset));

    void IVisitor.TopRef(Size offset) => Push(LocalData.Address(offset));

    internal IEnumerable<DataMemento> GetLocalItemMementos() => Data.GetItemMementos();

    void Push(StackData value) => Data = Data.Push(value);

    void TracerAssert(bool condition, Func<string> dumper)
    {
        if(TraceCollector == null)
        {
            condition.Assert(dumper, 1);
            return;
        }

        if(condition)
            return;

        TraceCollector?.AssertionFailed(dumper, 1);
    }

    void SubVisit(IFormalCodeItem codeBase)
    {
        if(TraceCollector == null)
            codeBase.Visit(this);
        else
            TraceCollector.Run(this, codeBase);
    }

    StackData Pull(Size size)
    {
        var result = Data.DoGetTop(size);
        Data = Data.DoPull(size);
        return result;
    }
}

interface IExecutionContext
{
    IOutStream? OutStream { get; }
    CodeBase Function(FunctionId functionId);
}

interface ITraceCollector
{
    void AssertionFailed(Func<string> dumper, int depth);
    void Run(DataStack dataStack, IFormalCodeItem codeBase);
    void Return();
    void Call(StackData argsAndRefs, FunctionId functionId);
}