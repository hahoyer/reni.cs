using Reni.Basics;

namespace Reni.Code;

abstract class StackData : DumpableObject
{
    internal readonly IOutStream OutStream;

    protected StackData(IOutStream outStream) => OutStream = outStream;

    internal abstract Size Size { get; }

    internal virtual StackData Push(StackData stackData)
    {
        NotImplementedMethod(stackData);
        return default!;
    }

    internal virtual StackData PushOnto(NonListStackData formerStack)
    {
        NotImplementedMethod(formerStack);
        return default!;
    }

    protected virtual StackData GetTop(Size size)
    {
        NotImplementedMethod(size);
        return default!;
    }

    protected virtual StackData Pull(Size size)
    {
        NotImplementedMethod(size);
        return default!;
    }

    internal virtual StackData PushOnto(ListStack formerStack)
    {
        NotImplementedMethod(formerStack);
        return default!;
    }

    internal virtual StackData BitArrayBinaryOp(string opToken, Size size, StackData right)
    {
        var leftData = GetBitsConst();
        var rightData = right.GetBitsConst();
        var resultData = leftData.BitArrayBinaryOp(opToken, size, rightData);
        return new BitsStackData(resultData, OutStream);
    }

    protected virtual StackDataAddress GetAddress()
    {
        NotImplementedMethod();
        return default!;
    }

    internal virtual BitsConst GetBitsConst()
    {
        NotImplementedMethod();
        return default!;
    }

    internal virtual StackData ForcedPull(Size size)
    {
        NotImplementedMethod(size);
        return default!;
    }

    internal virtual IEnumerable<DataStack.DataMemento> GetItemMementos()
    {
        yield return new(Dump())
        {
            Size = Size.ToInt()
        };
    }

    internal void PrintNumber() => GetBitsConst().PrintNumber(OutStream);
    internal void PrintText(Size itemSize) => GetBitsConst().PrintText(itemSize, OutStream);

    internal StackData DoGetTop(Size size)
    {
        if(size == Size)
            return this;
        if(size.IsZero)
            return new EmptyStackData(OutStream);
        var result = GetTop(size);
        (result.Size == size).Assert();
        return result;
    }

    internal StackData DoPull(Size size)
    {
        if(size.IsZero)
            return this;
        if(size == Size)
            return new EmptyStackData(OutStream);
        var result = Pull(size);
        (result.Size + size == Size).Assert();
        return result;
    }

    internal StackData BitCast(Size dataSize)
    {
        if(Size == dataSize)
            return this;

        return new BitsStackData(GetBitsConst().Resize(dataSize), OutStream);
    }

    internal StackData BitArrayPrefixOp(string operation, Size size)
    {
        var argData = GetBitsConst();
        var resultData = argData.BitArrayPrefixOp(operation, size);
        return new BitsStackData(resultData, OutStream);
    }

    internal StackData RefPlus(Size offset) => GetAddress().RefPlus(offset);

    internal StackData Dereference(Size size, Size dataSize)
        => GetAddress().Dereference(size, dataSize);

    internal void Assign(Size size, StackData right) => GetAddress().Assign(size, right);
}