using hw.Scanner;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP;

static class Extension
{
    public static string GetKey(this TextDocumentIdentifier target) => target.Uri.GetFileSystemPath();

    public static Range GetRange(this SourcePart token)
    {
        var range = token.TextPosition;
        return new(range.start.LineNumber, range.start.ColumnNumber1 - 1, range.end.LineNumber
            , range.end.ColumnNumber1 - 1);
    }

    public static bool? ToBoolean(this FormattingOptions option, string name)
    {
        if(option.ContainsKey(name) && option[name].IsBool)
            return option[name].Bool;
        return null;
    }

    public static int? ToInteger(this FormattingOptions option, string name)
    {
        if(option.ContainsKey(name) && option[name].IsInteger)
            return option[name].Integer;
        return null;
    }
}