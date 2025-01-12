using hw.Scanner;

namespace Reni.Parser;

sealed class SavePartMatch : DumpableObject, IMatch
{
    readonly IMatch Target;
    readonly Action<string> OnMatch;
    readonly Action OnMismatch;

    public SavePartMatch(IMatch target, Action<string> onMatch, Action onMismatch = null)
    {
        Target = target;
        OnMatch = onMatch;
        OnMismatch = onMismatch;
    }

    int? IMatch.Match(SourcePart span, bool isForward)
    {
        var length = span.Match(Target, isForward);
        if(length != null)
            OnMatch(span.GetStart(isForward).Span(length.Value).Id);
        else
            OnMismatch?.Invoke();
        return length;
    }
}