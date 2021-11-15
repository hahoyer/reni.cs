using System;
using System.ComponentModel.Composition;
using hw.DebugFormatter;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX
{
    [Export(typeof(IVsCommandHandlerServiceAdapter))]
    [ContentType("reni")] // This classifier applies to all text files.
    sealed class EditorCommands : DumpableObject, IOleCommandTarget
    {
        int IOleCommandTarget.Exec
            (ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            NotImplementedFunction(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            return default;
        }

        int IOleCommandTarget.QueryStatus
            (ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            NotImplementedFunction(pguidCmdGroup, cCmds, prgCmds, pCmdText);
            return default;
        }
    }
}