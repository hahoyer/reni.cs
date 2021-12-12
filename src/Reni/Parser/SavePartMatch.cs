using System;
using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.Parser
{
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

        int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
        {
            var length = sourcePosition.Match(Target, isForward);
            if (length != null)
                OnMatch(sourcePosition.SubString(0, length.Value));
            else
                OnMismatch?.Invoke();
            return length;
        }
    }
}