using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Configuration
    {
        public int? EmptyLineLimit;
        public int? MaxLineLength;
        public int IndentCount = 4;
        public bool SpaceBeforeListItem = false;
        public bool SpaceAfterListItem = true;
        public bool? LineBreakAtEndOfText = false;

        public bool IsLineBreakRequired(Helper.Syntax syntax)
            => syntax?.HasAlreadyLineBreakOrIsTooLong(MaxLineLength, EmptyLineLimit != 0) == true;
    }
}