using CommandLine;
using Reni;

namespace ReniExe;

static class MainContainer
{
    [UsedImplicitly]
    public sealed class Options
    {
        [Option('f', "fileName", Required = true, HelpText = "The path to file to compile.")]
        public string FileName { get; set; }
    }

    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                options.FileName.AssertIsNotNull();
                var p = new CompilerParameters
                {
                    OutStream = new ConsoleStream(), TraceOptions = { Parser = true }
                };

                var c = Compiler.FromFile(options.FileName, p);
                try
                {
                    c.Execute();
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            });

        return;
    }
}

sealed class ConsoleStream : DumpableObject, IOutStream
{
    void IOutStream.AddData(string text) => Console.Write(text);
    void IOutStream.AddLog(string text) => Console.Write(text);
}