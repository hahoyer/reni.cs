namespace ReniUI;

abstract class GuiExceptionGuard<TResult> : DumpableObject
{
    const string ExceptionTextFileName = "Exception.txt";
    const string SourceFileName = "source.reni";
    const string TestFileName = "Test.cs";
    protected readonly CompilerBrowser Parent;
    protected GuiExceptionGuard(CompilerBrowser parent) => Parent = parent;

    protected abstract string GetTestCode(string folderName);

    public abstract TResult OnException(Exception exception);

    protected void CreateDiscriminatingTest(Exception exception)
    {
        var generatedFolder = (SmbFile.SourceFolder ?? ".".ToSmbFile()) / "Generated";
        var sourceCode = @$"#( Source: {Parent.Source.Identifier} )#
{Parent.Source.Data}";

        var lastReading = generatedFolder.Items.Top(file => HasSourceCode(file, sourceCode));
        if(lastReading == null)
        {
            var folderName = $"At{DateTime.Now:yyMMdd_HHmmss}";
            var issueFolder = generatedFolder / folderName;

            SaveExceptionInformationFile(issueFolder, ExceptionTextFileName, Dump(exception), "Exception Data");
            SaveExceptionInformationFile(issueFolder, SourceFileName, sourceCode, "Source file ");
            SaveExceptionInformationFile(issueFolder, TestFileName, GetTestCode(folderName), "Test code");
        }
        else
            $"Exception already saved to: {Tracer.FilePosition(lastReading.FullName, 1, 1, FilePositionTag.Output)}"
                .Log();
    }

    static bool HasContent(SmbFile target, string content) 
        => (target / ExceptionTextFileName).String == content;

    static bool HasSourceCode(SmbFile target, string sourceCode)
        => (target / SourceFileName).String == sourceCode;

    static string Dump(Exception exception)
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