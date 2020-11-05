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
        public bool AdditionalLineBreaksForMultilineItems = true;
    }
}