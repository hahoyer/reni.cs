using hw.DebugFormatter;
using hw.Helper;
using Reni;
using ReniUI.Formatting;

namespace ReniUI.Test.Formatting;

static class Extension
{
    const string HorizontalLine = "\n----------------------------------\n";

    static string Canonize(this string target, char spaceReplacement = '_', bool replaceCarriageReturn = true)
    {
        if(replaceCarriageReturn)
            target = target.Replace("\r\n", "\n");
        if(spaceReplacement != '\0')
            target = target
                .Replace(" ", "")
                .Replace('_', ' ');
        return target;
    }

    public static void SimpleFormattingTest
    (
        this string text
        , string expected = null
        , int? maxLineLength = null
        , int? emptyLineLimit = null
        , int? indentCount = null
        , CompilerParameters parameters = null
        , char spaceReplacement = '\0'
        , bool? lineBreaksAtComplexDeclaration = null
    )
    {
        expected ??= text;
        expected = expected.Canonize(spaceReplacement);

        var compiler = CompilerBrowser.FromText(text.Canonize(spaceReplacement, false), parameters);
        var configuration = new ReniUI.Formatting.Configuration
            { MaxLineLength = maxLineLength, EmptyLineLimit = emptyLineLimit };
        if(indentCount != null)
            configuration.IndentCount = indentCount.Value;
        if(lineBreaksAtComplexDeclaration != null)
            configuration.LineBreaksAtComplexDeclaration = lineBreaksAtComplexDeclaration.Value;
        var newText = compiler.Reformat
        (
            configuration
                .Create()
        );

        string LocalCanonize(string target) => target.Replace("\r\n", "\n")
        //    .Replace("\n","|||\n")
        ;

        if(newText.Canonize(spaceReplacement) == expected.Canonize(spaceReplacement))
            return;

        GetDifferenceReport(LocalCanonize(newText.Canonize(spaceReplacement))
            , LocalCanonize(expected.Canonize(spaceReplacement))).Log();

        false.Assert(() => $@"                    
new:
----------------------
{LocalCanonize(newText)}
----------------------
expected:
----------------------
{LocalCanonize(expected)}
----------------------
", 1
        );
    }

    static string GetDifferenceReport(string text, string expected)
    {
        var lines = text.Split('\n');
        var expectedLines = expected.Split('\n');

        if(DateTime.Today.Year > 2020)
        {
            var items = Diff.DiffText(text, expected);
            return items
                .Select(item => FormatDifference(item, lines, expectedLines))
                .Stringify("\n");
        }

        return UtilDiff
            .diff_comm(lines, expectedLines)
            .Where(IsRelevant)
            .Select(FormatDifference)
            .Stringify("\n");
    }

    static string FormatDifference(UtilDiff.commonOrDifferentThing difference)
    {
        var sourcePart = difference
            .file1
            .Select(line => $"new: |{line}|")
            .Stringify("\n");
        var expectedSourcePart = difference
            .file2
            .Select(line => $"exp: |{line}|")
            .Stringify("\n");

        return HorizontalLine + sourcePart + HorizontalLine + expectedSourcePart + HorizontalLine;
    }

    static bool IsRelevant(UtilDiff.commonOrDifferentThing item) => item.file1 != null || item.file2 != null;

    static string FormatDifference(Diff.Item difference, string[] lines, string[] expectedLines)
    {
        var sourcePart = difference
            .DeletedA
            .Select(offset => $"[{difference.StartA + offset}] new: |" + lines[difference.StartA + offset] + "|")
            .Stringify("\n");
        var expectedSourcePart = difference
            .InsertedB
            .Select(offset
                => $"[{difference.StartB + offset}] exp: |" + expectedLines[difference.StartB + offset] + "|")
            .Stringify("\n");

        return HorizontalLine + sourcePart + HorizontalLine + expectedSourcePart + HorizontalLine;
    }
}