using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code;

abstract class Visitor<TCode, TFiber> : DumpableObject
{
    protected Visitor(int objectId)
        : base(objectId) { }

    protected Visitor() { }

    internal abstract TCode Default(CodeBase codeBase);

    internal virtual TCode Arg(Argument visitedObject)
    {
        NotImplementedMethod(visitedObject);
        return default;
    }

    internal virtual TCode ContextRef(ReferenceCode visitedObject)
    {
        NotImplementedMethod(visitedObject);
        return default;
    }

    internal virtual TCode LocalReference(LocalReference visitedObject)
    {
        NotImplementedMethod(visitedObject);
        return default;
    }

    internal virtual TCode BitArray(BitArray visitedObject)
    {
        NotImplementedMethod(visitedObject);
        return default;
    }

    internal virtual TCode Fiber(Fiber visitedObject)
    {
        var newHead = visitedObject.FiberHead.Visit(this);
        var data = visitedObject.FiberItems;
        var newItems = new TFiber[data.Length];
        for(var index = 0; index < data.Length; index++)
            newItems[index] = data[index].Visit(this);
        return Fiber(visitedObject, newHead, newItems);
    }

    protected virtual TCode Fiber(Fiber visitedObject, TCode newHead, TFiber[] newItems)
    {
        NotImplementedMethod(visitedObject, newHead, newItems);
        return default;
    }

    protected virtual Visitor<TCode, TFiber> After(Size size) => this;

    internal virtual TCode List(List visitedObject)
    {
        var visitor = this;
        var data = visitedObject.Data;
        var newList = new TCode[data.Length];
        for(var index = 0; index < data.Length; index++)
        {
            var codeBase = data[index];
            newList[index] = codeBase.Visit(visitor);
            visitor = visitor.AfterAny(codeBase.Size);
        }

        return visitor.List(visitedObject, newList);
    }

    protected virtual TCode List(List visitedObject, IEnumerable<TCode> newList)
    {
        NotImplementedMethod(visitedObject, newList);
        return default;
    }

    internal virtual TFiber ThenElse(ThenElse visitedObject)
    {
        var newThen = visitedObject.ThenCode.Visit(this);
        var newElse = visitedObject.ElseCode.Visit(this);
        return ThenElse(visitedObject, newThen, newElse);
    }

    protected virtual TFiber ThenElse(ThenElse visitedObject, TCode newThen, TCode newElse)
    {
        NotImplementedMethod(visitedObject, newThen, newElse);
        return default;
    }

    internal virtual TFiber Call(Call visitedObject) => default;
    internal virtual TCode TopRef(TopRef visitedObject) => default;
    internal virtual TCode TopFrameData(TopFrameData visitedObject) => default;
    internal virtual TCode TopData(TopData visitedObject) => default;
    internal virtual TFiber DePointer(DePointer visitedObject) => default;
    internal virtual TFiber Drop(Drop visitedObject) => default;
    internal virtual TFiber BitCast(BitCast visitedObject) => default;
    internal virtual TFiber BitArrayBinaryOp(BitArrayBinaryOp visitedObject) => default;
    internal virtual TFiber ArraySetter(ArraySetter visitedObject) => default;
    internal virtual TFiber ArrayGetter(ArrayGetter visitedObject) => default;
    internal virtual TFiber Assign(Assign visitedObject) => default;
    internal virtual TCode DumpPrintText(DumpPrintText visitedObject) => default;

    internal virtual TFiber ReferencePlusConstant(ReferencePlusConstant visitedObject)
        => default;

    internal virtual TFiber DumpPrintTextOperation(DumpPrintTextOperation visitedObject)
        => default;

    internal virtual TFiber DumpPrintNumberOperation(DumpPrintNumberOperation visitedObject)
        => default;


    Visitor<TCode, TFiber> AfterAny(Size size)
    {
        if(size.IsZero)
            return this;
        return After(size);
    }
}