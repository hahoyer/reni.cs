using hw.Scanner;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReniUI.Classification;

namespace ReniLSP;

static class Extension
{
    public static string GetKey(this TextDocumentIdentifier target) => target.Uri.GetFileSystemPath();

    public static IEnumerable<string> GetTypes(this Item item)
    {
        if(item.IsComment)
            yield return "comment";
        if(item.IsKeyword)
            yield return "keyword";
        if(item.IsNumber)
            yield return "number";
        if(item.IsText)
            yield return "string";
        if(item.IsIdentifier)
            yield return "variable";
    }

    public static OmniSharp.Extensions.LanguageServer.Protocol.Models.Range GetRange(this SourcePart token)
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