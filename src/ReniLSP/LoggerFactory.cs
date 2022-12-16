using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using hw.DebugFormatter;
using hw.Helper;
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
            $"{CategoryName}: [{logLevel}] {formatter(state, exception)}".LogLinePart();
            TraceException("Exception", exception).Log();
        }

        static string TraceException(string title, Exception exception)
            => exception == null
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