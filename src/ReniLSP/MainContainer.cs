using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;

namespace ReniLSP
{
    static class MainContainer
    {

        public static async Task Main(string[] args)
        {
            if(args.SingleOrDefault() == "none")
            {
                "Doing nothing".Log();
                return;
            }

            Debugger.Launch();

            var server = await LanguageServer.From(options
                => options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .WithLoggerFactory(new LoggerFactory())
                    .AddDefaultLoggingProvider()
                    .OnInitialize(Initialize)
                    .OnInitialized(Initialized)
                    .WithHandler<Target>()
                    .WithHandler<TokenTarget>()
            );
            await server.WaitForExit;
        }

        static Task Initialized
        (
            ILanguageServer server, InitializeParams request, InitializeResult response
            , CancellationToken cancellationtoken
        )
        {
            Dumpable.NotImplementedFunction(server, request, response, cancellationtoken);
            return Task.CompletedTask;
        }

        static Task Initialize(ILanguageServer server, InitializeParams request, CancellationToken cancellationtoken)
        {
            Dumpable.NotImplementedFunction(server, request, cancellationtoken);
            return Task.CompletedTask;
        }


        internal static FilePositionTag ToFilePositionTag(this LogLevel level)
            => level switch
            {
                LogLevel.Trace => FilePositionTag.Debug
                , LogLevel.Debug => FilePositionTag.Debug
                , _ => FilePositionTag.Output
            };
    }

    class LoggerFactory : DumpableObject, ILoggerFactory
    {
        class Logger : DumpableObject, ILogger
        {
            readonly string CategoryName;
            public Logger(string categoryName) => CategoryName = categoryName;

            IDisposable ILogger.BeginScope<TState>(TState state) => null;

            bool ILogger.IsEnabled(LogLevel logLevel) => true;

            void ILogger.Log<TState>
            (
                LogLevel logLevel, EventId eventId, TState state, Exception? exception
                , Func<TState, Exception?, string> formatter
            ) => ($"{CategoryName}: [{logLevel}] {formatter(state, exception)}").Log();
        }

        void IDisposable.Dispose() { }

        void ILoggerFactory.AddProvider(ILoggerProvider provider) => NotImplementedMethod(provider);

        ILogger ILoggerFactory.CreateLogger(string categoryName) => new Logger(categoryName);
    }
}