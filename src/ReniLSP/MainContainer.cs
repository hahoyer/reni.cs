using System.IO.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;
using Reni.Helper;

namespace ReniLSP;

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
        => await RunOmniServer(reader, writer);

    static async Task RunOmniServer(PipeReader reader, PipeWriter writer)
    {
        try
        {
            Expectations.BreakMode = Expectations.BreakModeType.UseException;

            void configureOptions(LanguageServerOptions options) => options
                .WithInput(reader)
                .WithOutput(writer)
                .WithLoggerFactory(new LoggerFactory())
                .AddDefaultLoggingProvider()
                .OnInitialized(Initialized)
                .WithHandler<MainWrapper>()
                .WithServices(services
                    => services
                        .AddLogging(b => b.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information))
                        .AddSingleton(
                            new ConfigurationItem
                            {
                                Section = "reni",
                            })
                )
            ;

            await LanguageServer.From(configureOptions);
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