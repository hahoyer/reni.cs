using System.Collections.Generic;
using hw.DebugFormatter;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ReniVSIX;

sealed class CompletionSource : DumpableObject, ICompletionSource
{
    readonly ITextBuffer TextBuffer;

    public CompletionSource(ITextBuffer textBuffer) => TextBuffer = textBuffer;

    public void AugmentCompletionSession
        (ICompletionSession session, IList<CompletionSet> completionSets)
        => NotImplementedMethod(session, completionSets);
}