using Reni.Basics;
using Reni.Feature;
using Reni.Type;

namespace Reni.Code;

sealed class List : FiberHead
{
    static int NextObjectId;

    [Node]
    internal CodeBase[] Data { get; }

    List(IEnumerable<CodeBase> data)
        : base(NextObjectId++)
    {
        Data = data.ToArray();
        AssertValid();
        StopByObjectIds();
    }

    internal override IEnumerable<CodeBase> ToList() => Data;

    protected override TCode? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TCode : default
        => actual.List(this);

    protected override CodeBase TryToCombine(FiberItem subsequentElement)
    {
        if(!IsNonFiberHeadList)
            return subsequentElement.TryToCombineBack(this);
        var newData = new CodeBase[Data.Length];
        var i = 0;
        for(; i < Data.Length - 1; i++)
            newData[i] = Data[i];
        newData[i] = Data[i].Concat(subsequentElement);
        return newData.GetCode();
    }

    [DisableDump]
    internal override bool IsNonFiberHeadList
    {
        get
        {
            for(var i = 0; i < Data.Length - 1; i++)
                if(!Data[i].IsHollow)
                    return false;
            return true;
        }
    }

    protected override Size GetTemporarySize()
    {
        var result = Size.Zero;
        var sizeSoFar = Size.Zero;
        foreach(var codeBase in Data)
        {
            var newResult = sizeSoFar + codeBase.TemporarySize;
            sizeSoFar += codeBase.Size;
            result = result.GetMax(newResult).GetMax(sizeSoFar);
        }

        return result;
    }

    protected override Size GetSize()
        => Data
            .Aggregate(Size.Zero, (size, codeBase) => size + codeBase.Size);

    protected override Closures GetClosures() => Data.GetClosures();
    internal override void Visit(IVisitor visitor) => visitor.List(Data);

    internal static CodeBase Create(params CodeBase?[] data)
    {
        var actualData = data.Where(item => item != null).Cast<CodeBase>().ToArray();
        return actualData.Length switch
        {
            0 => Void, 1 => actualData[0], var _ => new List(actualData)
        };
    }

    internal static CodeBase? CheckedCreate(IEnumerable<CodeBase>? data)
    {
        if(data == null)
            return null;

        var dataArray = data.ToArray();

        return dataArray.Length switch
        {
            0 => null, 1 => dataArray[0], var _ => new List(dataArray)
        };
    }

    void AssertValid()
    {
        foreach(var codeBase in Data)
        {
            (!(codeBase is List)).Assert(() => codeBase.Dump());
            (!codeBase.IsEmpty).Assert();
        }

        (Data.Length > 1).Assert();
    }

    internal bool IsCombinePossible(RecursiveCallCandidate recursiveCallCandidate)
    {
        if(!(recursiveCallCandidate.DeltaSize + Size).IsZero)
            return false;

        var topFrameDatas = Data.Select(element => element as TopFrameData).ToArray();
        if(topFrameDatas.Any(element => element == null))
            return false;

        var size = Size;
        foreach(var topFrameData in topFrameDatas)
        {
            if(topFrameData!.Size + topFrameData.Offset != size)
                return false;
            size -= topFrameData.Size;
        }

        return size.IsZero;
    }

    internal TypeBase? Visit(Visitor<TypeBase, TypeBase> argTypeVisitor)
        => Data
            .Select(x => x.Visit(argTypeVisitor))
            .DistinctNotNull();
}