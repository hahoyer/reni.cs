using hw.Helper;

namespace ReniUI.Formatting
{
    sealed class FormatterMode: EnumEx
    {
        public static readonly FormatterMode None = new FormatterMode();
        public static readonly FormatterMode ForcedLineBreaks = new FormatterMode {HasLineBreakForced = true};
        public static readonly FormatterMode Root = new FormatterMode {IsRoot = true};
        public bool HasLineBreakForced;
        public bool IsRoot;

        FormatterMode() {}
    }
}