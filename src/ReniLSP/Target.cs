using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.JsonRpc.Server;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP
{
    class Target : DumpITextDocumentSyncHandler
    {
        Task<Unit> IRequestHandler<DidChangeTextDocumentParams, Unit>.Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {

            throw new System.NotImplementedException();
        }

        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
            => throw new System.NotImplementedException();

        Task<Unit> IRequestHandler<DidOpenTextDocumentParams, Unit>.Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken) => throw new System.NotImplementedException();

        TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
            => throw new System.NotImplementedException();

        Task<Unit> IRequestHandler<DidCloseTextDocumentParams, Unit>.Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken) => throw new System.NotImplementedException();

        TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
            => throw new System.NotImplementedException();

        Task<Unit> IRequestHandler<DidSaveTextDocumentParams, Unit>.Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken) => throw new System.NotImplementedException();

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
            => throw new System.NotImplementedException();

        TextDocumentAttributes ITextDocumentIdentifier.GetTextDocumentAttributes(DocumentUri uri) => throw new System.NotImplementedException();
    }
}