using hw.DebugFormatter;

namespace ReniUI.Formatting;

sealed class Configuration : DumpableObject
{
    public int? EmptyLineLimit;
    public int? MaxLineLength;
    public int IndentCount = 4;
    public bool? LineBreakAtEndOfText = false;
    public bool AdditionalLineBreaksForMultilineItems = true;
    public bool LineBreaksBeforeListToken;
    public bool LineBreaksBeforeDeclarationToken;
    public bool LineBreakAtComplexDeclarationValue;
    public string LineBreakString = "\r\n";
}