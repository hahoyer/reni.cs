﻿using Reni.Basics;

namespace Reni.Code;

abstract class FiberItem : DumpableObject, IFormalCodeItem
{
    static int NextObjectId;

    [DisableDump]
    [field: EnableDumpExcept("")]
    [field: EnableDump]
    string ReasonForCombine => field == ""? NodeDumpForDebug() : field;

    [DisableDump]
    static string? NewCombinedReason
    {
        get;
        set
        {
            (field == null != (value == null)).Assert();
            field = value;
        }
    }

    [DisableDump]
    internal Size DeltaSize => OutputSize - InputSize;

    [DisableDump]
    string DumpSignature => "(" + InputSize + "==>" + OutputSize + ")";

    [DisableDump]
    internal Closures Closures => GetRefsImplementation();

    [DisableDump]
    internal Size TemporarySize => OutputSize + GetAdditionalTemporarySize();

    protected FiberItem(int objectId, string? reason = null)
        : base(objectId)
        => ReasonForCombine = reason ?? NewCombinedReason ?? "";

    protected FiberItem(string? reason = null)
        : this(NextObjectId++, reason) { }

    Size IFormalCodeItem.Size => DeltaSize;

    void IFormalCodeItem.Visit(IVisitor visitor) => Visit(visitor);

    [DisableDump]
    internal abstract Size InputSize { get; }

    [DisableDump]
    internal abstract Size OutputSize { get; }

    internal abstract void Visit(IVisitor visitor);

    [DisableDump]
    internal virtual bool HasArgument => false;

    protected virtual Size GetAdditionalTemporarySize() => Size.Zero;

    protected virtual FiberItem[]? TryToCombineImplementation(FiberItem subsequentElement)
        => null;

    internal virtual CodeBase? TryToCombineBack(BitArray precedingElement) => null;
    internal virtual CodeBase? TryToCombineBack(TopFrameRef precedingElement) => null;
    internal virtual CodeBase? TryToCombineBack(TopData precedingElement) => null;
    internal virtual CodeBase? TryToCombineBack(TopFrameData precedingElement) => null;
    internal virtual CodeBase? TryToCombineBack(TopRef precedingElement) => null;
    internal virtual CodeBase? TryToCombineBack(List precedingElement) => null;
    internal virtual CodeBase? TryToCombineBack(LocalReference precedingElement) => null;
    internal virtual FiberItem[]? TryToCombineBack(IdentityTestCode precedingElement) => null;
    internal virtual FiberItem[]? TryToCombineBack(BitArrayBinaryOp precedingElement) => null;
    internal virtual FiberItem[]? TryToCombineBack(BitArrayPrefixOp precedingElement) => null;
    internal virtual FiberItem[]? TryToCombineBack(BitCast preceding) => null;
    internal virtual FiberItem[]? TryToCombineBack(DePointer preceding) => null;

    internal virtual FiberItem[]? TryToCombineBack(ReferencePlusConstant precedingElement)
        => null;

    protected virtual TFiber? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        => default;

    protected virtual Closures GetRefsImplementation() => Closures.GetVoid();

    protected override string GetNodeDump() => base.GetNodeDump() + DumpSignature;

    internal FiberItem[]? TryToCombine(FiberItem subsequentElement)
    {
        NewCombinedReason = ReasonForCombine + " " + subsequentElement.ReasonForCombine;
        var result = TryToCombineImplementation(subsequentElement);
        NewCombinedReason = null;
        return result;
    }

    internal TFiber? Visit<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        => VisitImplementation(actual);
}

interface IFormalCodeItem
{
    void Visit(IVisitor visitor);
    string Dump();
    Size Size { get; }
}