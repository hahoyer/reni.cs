namespace Reni.Code;

sealed class ItemCollector
    : Visitor<IEnumerable<IFormalCodeItem>, IEnumerable<IFormalCodeItem>>
{
    internal override IEnumerable<IFormalCodeItem> BitArray(BitArray visitedObject) { yield return visitedObject; }

    protected override IEnumerable<IFormalCodeItem> Fiber
    (
        Fiber visitedObject,
        IEnumerable<IFormalCodeItem> newHead,
        IEnumerable<IFormalCodeItem>[] newItems
    )
    {
        yield return visitedObject;

        if(newHead == null)
            yield return visitedObject.FiberHead;
        else
            foreach(var item in newHead)
                yield return item;

        var codeItems = newItems
            .Select((item, index) => item ?? new[] { visitedObject.FiberItems[index] })
            .SelectMany(i => i);
        foreach(var item in codeItems)
            yield return item;
    }

    protected override IEnumerable<IFormalCodeItem> List
        (List visitedObject, IEnumerable<IEnumerable<IFormalCodeItem>> newList)
    {
        yield return visitedObject;
        var codeItems = newList
            .Select((item, index) => item ?? new[] { visitedObject.Data[index] })
            .SelectMany(i => i);
        foreach(var item in codeItems)
            yield return item;
    }

    internal override IEnumerable<IFormalCodeItem> TopFrameData(TopFrameData visitedObject)
    {
        yield return visitedObject;
    }

    internal override IEnumerable<IFormalCodeItem> Default(CodeBase codeBase) { yield return codeBase; }

    protected override IEnumerable<IFormalCodeItem> ThenElse
    (
        ThenElse visitedObject,
        IEnumerable<IFormalCodeItem> newThen,
        IEnumerable<IFormalCodeItem> newElse
    )
    {
        yield return visitedObject;
        foreach(var item in newThen)
            yield return item;
        foreach(var item in newElse)
            yield return item;
    }
}