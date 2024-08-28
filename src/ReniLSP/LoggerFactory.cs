using System.Diagnostics;
using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace ReniLSP;

sealed class LoggerFactory : DumpableObject, ILoggerFactory
{
    sealed class Logger : DumpableObject, ILogger
    {
        readonly string CategoryName;
        public Logger() => Debugger.Launch();
        public Logger(string categoryName) => CategoryName = categoryName;

        IDisposable ILogger.BeginScope<TState>(TState state) => new CompositeDisposable();

        bool ILogger.IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>
        (
            LogLevel logLevel, EventId eventId, TState state, Exception exception
            , Func<TState, Exception, string> formatter
        )
        {
            //if(logLevel.In(LogLevel.Debug, LogLevel.Trace))
            //    return;
            $"{CategoryName}: [{logLevel}] {formatter(state, exception)}".LogLinePart();
            TraceException("Exception", exception).Log();
        }

        static string TraceException(string title, Exception exception) => exception == null
            ? ""
            : @$"
{title}: {exception.Message}
    {exception.Source}
    {exception.StackTrace}
    {TraceException("Inner Exception", exception.InnerException).Indent()}";
    }

    void IDisposable.Dispose() { }

    void ILoggerFactory.AddProvider(ILoggerProvider provider) => NotImplementedMethod(provider);

    ILogger ILoggerFactory.CreateLogger(string categoryName) => new Logger(categoryName);
}

/*
    $/setTrace:OmniSharp.Extensions.LanguageServer.Server.Logging.LanguageServerLoggingManager,
    $/progress:OmniSharp.Extensions.LanguageServer.Protocol.Progress.ProgressManager,


    initialize:OmniSharp.Extensions.LanguageServer.Server.LanguageServer,
    initialized:OmniSharp.Extensions.LanguageServer.Server.LanguageServer
    textDocument/didOpen:ReniLSP.MainWrapper,
    textDocument/didChange:ReniLSP.MainWrapper,
    textDocument/didClose:ReniLSP.MainWrapper,
    textDocument/formatting:ReniLSP.MainWrapper,
    textDocument/semanticTokens/range:ReniLSP.MainWrapper,
    textDocument/semanticTokens/full/delta:ReniLSP.MainWrapper,
    textDocument/semanticTokens/full:ReniLSP.MainWrapper,
    window/workDoneProgress/cancel:OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone.LanguageServerWorkDoneManager,
    workspace/configuration:ReniLSP.MainWrapper,
    workspace/didChangeWorkspaceFolders:OmniSharp.Extensions.LanguageServer.Server.LanguageServerWorkspaceFolderManager,
    workspace/didChangeConfiguration:ReniLSP.MainWrapper,
    shutdown:OmniSharp.Extensions.LanguageServer.Server.LanguageServer,
    exit:OmniSharp.Extensions.LanguageServer.Server.LanguageServer,

OmniSharp.Extensions.JsonRpc.InputHandler: [Debug] Notification handler was not found (or not setup) NotificationReceived

*/