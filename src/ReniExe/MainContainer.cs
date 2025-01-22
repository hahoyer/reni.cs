using Reni;

namespace ReniExe;

static class MainContainer
{
    public static void Main(string[] args)
    {
        if(args.Length != 1)
        {
            Console.WriteLine("usage: ReniExe <filename>");
            return;
        }

        var p = new CompilerParameters
        {
            OutStream = new ConsoleStream()
        };
        var c = Compiler.FromFile(args[0], p);
        try
        {
            c.Execute();
        }
        catch(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}

sealed class ConsoleStream : DumpableObject, IOutStream
{
    void IOutStream.AddData(string text) => Console.Write(text);
    void IOutStream.AddLog(string text) => Console.Write(text);
}