namespace ReniUI.Formatting;

sealed class Configuration : DumpableObject
{
    public int? EmptyLineLimit;
    public int? MaxLineLength;
    public int IndentCount = 2;
    public bool? LineBreakAtEndOfText = false;
    public bool AdditionalLineBreaksForMultilineItems = true;
    public bool LineBreaksBeforeListToken;
    public bool LineBreaksAtComplexDeclaration;
    public string LineBreakString = "\r\n";
}