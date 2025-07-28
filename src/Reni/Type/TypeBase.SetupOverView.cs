using System.Diagnostics;
using Reni.Basics;

namespace Reni.Type;

abstract partial class TypeBase
{
    internal sealed class SetupOverView(TypeBase parent)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly TypeBase Parent = parent;

        internal bool HasIssues => Parent.Issues.Any();
        internal Size Size => Parent.Cache.Size.Value;
        internal Size? SmartSize => Parent.Cache.Size.IsBusy? null : Size;
        internal bool IsHollow => Parent.GetIsHollow();
        internal bool IsAligningPossible => Parent.GetIsAligningPossible();
        internal bool IsPointerPossible => Parent.GetIsPointerPossible();
        internal bool IsWeakReference => Parent.Make.CheckedReference != null && Parent.Make.CheckedReference.IsWeak;
        internal bool HasQuickSize => Parent.GetHasQuickSize();
        internal string DumpPrintText => Parent.GetDumpPrintText();
    }
}
