using Reni.Basics;

namespace Reni.Code;

/// <summary>
///     Code element for a call that has been resolved as simple recursive call candidate. This implies, that the call is
///     contained in the function called. It must not have any argument and should return nothing. It will be assembled as
///     a jump to begin of function.
/// </summary>
sealed class RecursiveCallCandidate : FiberItem
{
    internal override Size InputSize { get; }

    internal override Size OutputSize => Size.Zero;

    internal override void Visit(IVisitor visitor) => visitor.RecursiveCallCandidate();

    internal override CodeBase? TryToCombineBack(TopFrameData precedingElement)
    {
        if((DeltaSize + precedingElement.Size).IsZero
           && precedingElement.Offset.IsZero)
            return new RecursiveCall();
        return base.TryToCombineBack(precedingElement);
    }

    internal override CodeBase? TryToCombineBack(List precedingElement)
    {
        if (precedingElement.IsCombinePossible(this))
            return new RecursiveCall();
        return base.TryToCombineBack(precedingElement);
    }

    internal RecursiveCallCandidate(Size refsSize) { InputSize = refsSize; }
}