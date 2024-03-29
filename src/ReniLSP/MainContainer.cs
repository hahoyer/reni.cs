using System.IO.Pipelines;
using hw.DebugFormatter;
using Nerdbank.Streams;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;

namespace ReniLSP;

public static class MainContainer
{
    public static async Task Main(string[] args)
    {
        if(args.SingleOrDefault() == "none")
        {
            "Doing nothing".Log();
            return;
        }

        await RunServer(Console.OpenStandardInput().UsePipeReader(), Console.OpenStandardOutput().UsePipeWriter());
    }

    public static async Task RunServer(PipeReader reader, PipeWriter writer)
    {
        try
        {
            var server = await LanguageServer.From(options
                => options
                    .WithInput(reader)
                    .WithOutput(writer)
                    .WithLoggerFactory(new LoggerFactory())
                    .AddDefaultLoggingProvider()
                    .OnInitialized(Initialized)
                    .WithHandler<MainWrapper>()
            );

            await server.WaitForExit;
        }
        catch(Exception)
        {
            Tracer.TraceBreak();
        }
    }

    static Task Initialized
    (
        ILanguageServer server, InitializeParams request, InitializeResult response
        , CancellationToken cancellationToken
    )
    {
        var options = response.Capabilities.SemanticTokensProvider;
        options.AssertIsNotNull();
        options.Full = true;
        options.Range = true;
        return Task.CompletedTask;
    }
}