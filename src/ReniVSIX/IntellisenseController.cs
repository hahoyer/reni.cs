using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ReniVSIX;

sealed class IntellisenseController : DumpableObject, IIntellisenseController
{
    readonly ITextView View;
    readonly IList<ITextBuffer> Buffers;

    public IntellisenseController(ITextView view, IList<ITextBuffer> buffers)
    {
        View = view;
        Buffers = buffers;
    }

    void IIntellisenseController.ConnectSubjectBuffer(ITextBuffer subjectBuffer) => throw new NotImplementedException();

    void IIntellisenseController.Detach(ITextView textView) { }

    void IIntellisenseController.DisconnectSubjectBuffer
        (ITextBuffer subjectBuffer) { }
}