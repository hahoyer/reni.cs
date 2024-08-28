using System.Diagnostics;
using System.IO.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;

namespace ReniLSP;

[PublicAPI]
public class Startup
{
    public async Task<object> Invoke(object input)
    {
        Debugger.Launch();
        await MainContainer.RunServer();
        return Task.CompletedTask;
    }
}

public static class MainContainer
{
    public static Task<object> Main(string[] args)
    {
        if(args.SingleOrDefault() != "none")
            return Task.FromResult<object>(RunServer());

        "Doing nothing".Log();
        return Task.FromResult<object>(Task.CompletedTask);
    }

    internal static async Task RunServer()
    {
        var reader = Console.OpenStandardInput();
        var writer = Console.OpenStandardOutput();
        await RunServer(reader.UsePipeReader(), writer.UsePipeWriter());
    }

    public static async Task RunServer(PipeReader reader, PipeWriter writer)
    {
        //var pipeReader = new MyReader("reader", reader);
        await RunOmniServer(reader, writer);
    }

    //await RunMyServer(reader, writer);
    static async Task RunMyServer(PipeReader reader, PipeWriter writer)
    {
        try
        {
            await Implementation.LanguageServer.Run(reader, writer);
        }
        catch(Exception)
        {
            Tracer.TraceBreak();
        }
    }

    static async Task RunOmniServer(PipeReader reader, PipeWriter writer)
    {
        try
        {
            void ConfigureOptions(LanguageServerOptions options) => options
                .WithInput(reader)
                .WithOutput(writer)
                .WithLoggerFactory(new LoggerFactory())
                .AddDefaultLoggingProvider()
                .OnInitialized(Initialized)
                .WithHandler<MainWrapper>()
                .WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Debug)))
            ;

            await LanguageServer.From(ConfigureOptions);
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
        response.Capabilities.SemanticTokensProvider = SemanticTokensHandlerWrapper.Capabilities;
        response.Capabilities.DocumentFormattingProvider = new(true);


        //response.Capabilities.DocumentHighlightProvider = new(true);

        return Task.CompletedTask;
    }
}