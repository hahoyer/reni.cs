using System;
using System.Collections.Generic;
using System.Linq;
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

        int? IMatch.Match(SourcePosn sourcePosn)
        {
            var length = sourcePosn.Match(Target);
            if (length != null)
                OnMatch(sourcePosn.SubString(0, length.Value));
            else
                OnMismatch?.Invoke();
            return length;
        }
    }
}