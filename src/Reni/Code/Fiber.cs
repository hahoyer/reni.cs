using Reni.Basics;
using Reni.Feature;
using Reni.Type;

namespace Reni.Code;

sealed class Fiber : CodeBase
{
    static int NextObjectId;

    [Node]
    internal readonly FiberHead FiberHead;

    [Node]
    internal readonly FiberItem[] FiberItems;

    [DisableDump]
    internal bool HasArgument => FiberHead.HasArguments || FiberItems.Any(x => x.HasArgument);

    internal Fiber(FiberHead fiberHead, FiberItem fiberItem)
        : this(fiberHead, null, fiberItem) { }

    Fiber(FiberHead fiberHead, IEnumerable<FiberItem>? fiberItems, FiberItem? fiberItem)
        : base(NextObjectId++)
    {
        FiberHead = fiberHead;
        var l = new List<FiberItem>();
        if(fiberItems != null)
            l.AddRange(fiberItems);
        if(fiberItem != null)
            l.Add(fiberItem);
        FiberItems = l.ToArray();
        AssertValid();

        StopByObjectIds();
    }

    internal override bool IsRelativeReference => FiberHead.IsRelativeReference;

    protected override Size GetTemporarySize()
    {
        var result = FiberHead.TemporarySize;
        var sizeSoFar = FiberHead.Size;
        foreach(var codeBase in FiberItems)
        {
            sizeSoFar -= codeBase.InputSize;
            var newResult = sizeSoFar + codeBase.TemporarySize;
            sizeSoFar += codeBase.OutputSize;
            result = result.GetMax(newResult).GetMax(sizeSoFar);
        }

        return result;
    }

    protected override Size GetSize() => FiberItems.Last().OutputSize;

    protected override Closures GetClosures()
        => FiberItems
            .Aggregate
                (FiberHead.Closures, (current, fiberItem) => current.Sequence(fiberItem.Closures));

    internal override CodeBase Concat(FiberItem subsequentElement)
    {
        var lastFiberItems = new List<FiberItem>
        {
            subsequentElement
        };
        var fiberItems = new List<FiberItem>(FiberItems);
        while(lastFiberItems.Count > 0)
            if(fiberItems.Count > 0)
            {
                var newLastFiberItems = fiberItems.Last().TryToCombine(lastFiberItems[0]);
                if(newLastFiberItems == null)
                {
                    fiberItems.AddRange(lastFiberItems);
                    lastFiberItems.RemoveAll(x => true);
                }
                else
                {
                    fiberItems.Remove(fiberItems.Last());
                    fiberItems.AddRange(newLastFiberItems);
                    lastFiberItems.RemoveAt(0);
                }
            }
            else
            {
                fiberItems.AddRange(lastFiberItems);
                lastFiberItems.RemoveAll(x => true);
            }

        if(fiberItems.Count <= 0)
            return FiberHead;
        return new Fiber(FiberHead, fiberItems, null);
    }

    protected override TCode? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TCode : default
        => actual.Fiber(this);

    internal override void Visit(IVisitor visitor)
        => visitor.Fiber(FiberHead, FiberItems);

    public override string DumpData()
    {
        var result = "";
        result += "[*] " + FiberHead.Dump() + "\n";
        result += FiberItems.DumpLines();
        return result.Substring(0, result.Length - 1);
    }

    void AssertValid()
    {
        (!FiberHead.IsNonFiberHeadList).Assert(Dump);
        FiberItems.Any().Assert(Dump);
        var lastSize = FiberHead.Size;
        foreach(var t in FiberItems)
        {
            (lastSize == t.InputSize).Assert(Dump);
            lastSize = t.OutputSize;
        }
    }

    internal CodeBase ReCreate(CodeBase? newHead, FiberItem?[] newItems)
        => (newHead ?? FiberHead)
            .AddRange(newItems.Select((x, i) => x ?? FiberItems[i]));

    internal TypeBase? Visit(Visitor<TypeBase, TypeBase> argTypeVisitor)
        => new[] { FiberHead.Visit(argTypeVisitor) }
            .Concat(FiberItems.Select(x => x.Visit(argTypeVisitor)))
            .DistinctNotNull();
}