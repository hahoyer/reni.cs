using hw.Helper;

namespace Reni;

public sealed class T4Compiler
{
    readonly string Text;

    public T4Compiler(string text) => Text = text;

    public string Code()
    {
        var fileName = Environment.GetEnvironmentVariable("temp") + "\\reni\\T4Compiler.reni";
        var f = fileName.ToSmbFile();
        f.String = Text;
        var compiler = Compiler.FromFile(fileName);
        return compiler.CSharpString;
    }
}