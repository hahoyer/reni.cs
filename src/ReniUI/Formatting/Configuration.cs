using hw.DebugFormatter;
using hw.Helper;

namespace ReniUI.Formatting
{
    sealed class Configuration: DumpableObject
    {
        public int? EmptyLineLimit;
        public int? MaxLineLength;
        public int IndentCount = 4;
        public bool SpaceBeforeListItem = false;
        public bool SpaceAfterListItem = true;
        public bool? LineBreakAtEndOfText = false;
        public bool AdditionalLineBreaksForMultilineItems = true;
        public bool LineBreaksBeforeListToken;
        public bool LineBreaksBeforeDeclarationToken;

        public string Indent=> " ".Repeat(IndentCount);
    }
}