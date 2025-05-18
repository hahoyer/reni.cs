using System.Diagnostics;
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
        [Option('s', "reniSystem", Required = true, HelpText = "The path to file to the system directory.")]
        public string ReniSystem { get; set; }
        [Option('P', "trace.Parser", Required = false, HelpText = "Trace parser.")]
        public bool TraceParser { get; set; }
    }

    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                options.FileName.AssertIsNotNull();
                var p = new CompilerParameters
                {
                    OutStream = new ConsoleStream(), TraceOptions = { Parser = options.TraceParser}
                };

                var c = Compiler.FromFiles([options.ReniSystem, options.FileName], p);
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