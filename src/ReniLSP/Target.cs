#nullable enable
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;
using JetBrains.Annotations;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP;

[UsedImplicitly]
sealed class Target : DumpableObject, IDidOpenTextDocumentHandler
{
    // ReSharper disable once CollectionNeverQueried.Local
    readonly ConcurrentDictionary<string, TextDocumentItem> Buffers = new();

    TextDocumentOpenRegistrationOptions
        IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => new();

    Task<Unit> IRequestHandler<DidOpenTextDocumentParams, Unit>.Handle
        (DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        var item = request.TextDocument;
        Buffers[item.Uri.GetFileSystemPath()] = item;
        return Unit.Task;
    }
}