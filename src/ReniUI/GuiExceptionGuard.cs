using hw.DebugFormatter;
using hw.Helper;

namespace ReniUI;

abstract class GuiExceptionGuard : DumpableObject
{
    protected readonly CompilerBrowser Parent;
    protected GuiExceptionGuard(CompilerBrowser parent) => Parent = parent;

    protected abstract string GetTestCode(string folderName);

    public TResult Run<TResult>(Exception exception, CompilerBrowser compiler)
    {
        var sourceFolder = SmbFile.SourceFolder ?? ".".ToSmbFile();
        var folderName = $"At{DateTime.Now:yyMMdd_HHmmss}";
        var issueFolder = sourceFolder / "Generated" / folderName;

        SaveExceptionInformationFile(issueFolder, "Exception.txt", Dump(exception), "Exception Data");
        SaveExceptionInformationFile(issueFolder, "Test.reni", @$"#( Source: {Parent.Source.Identifier} )#
{Parent.Source.Data}", "Source file ");
        SaveExceptionInformationFile(issueFolder, "Text.cs", GetTestCode(folderName), "Test code");
        return default;
    }

    public static string Dump(Exception exception)
    {
        if(exception == null)
            return "";
        var innerExceptionDump = Dump(exception.InnerException);
        if(innerExceptionDump.Length > 0)
            innerExceptionDump = $@"
InnerException: {innerExceptionDump}".Indent();

        return @$"{exception.GetType().PrettyName()}: {exception.Message}
{exception.StackTrace}{innerExceptionDump}";
    }

    static void SaveExceptionInformationFile(SmbFile folder, string name, string content, string title)
    {
        var file = folder / name;
        file.String = content;
        $"{title} saved to :\n{Tracer.FilePosition(file.FullName, 1, 1, FilePositionTag.Output)}".Log();
    }
}

sealed class SyntaxExceptionGuard : GuiExceptionGuard
{
    public SyntaxExceptionGuard(CompilerBrowser parent)
        : base(parent) { }

    protected override string GetTestCode(string folderName) => @$"
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace ReniUI.Generated.{folderName};

[UnitTest]
public class Test : CompilerTest
{{
    protected override string Target => (SmbFile.SourceFolder / ""Text.reni"").String;

    protected override void Run()
    {{
        base.Run<()>()
}}
";
}

sealed class IssuesExceptionGuard : GuiExceptionGuard
{
    public IssuesExceptionGuard(CompilerBrowser parent)
        : base(parent) { }

    protected override string GetTestCode(string folderName) => @$"
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace ReniUI.Generated.{folderName};

[UnitTest]
public class Test : CompilerTest
{{
    protected override string Target => (SmbFile.SourceFolder / ""Text.reni"").String;

    protected override void Verify(IEnumerable<Issue> issues)
    {{
        var issueArray = issues.ToArray();
        var i = 0;
        //var issueBase = issueArray[i];
        //(issueBase.IssueId == IssueId.MissingDeclarationValue).Assert(issueBase.Dump);
        //i++;
        (i == issueArray.Length).Assert();
    }}
}}
";
}