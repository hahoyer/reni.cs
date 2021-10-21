using System;
using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP
{
    class Target : DumpableObject, ITextDocumentSyncHandler
    {
        TextDocumentChangeRegistrationOptions
            IRegistration<TextDocumentChangeRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        TextDocumentCloseRegistrationOptions
            IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        TextDocumentOpenRegistrationOptions
            IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        TextDocumentSaveRegistrationOptions
            IRegistration<TextDocumentSaveRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
            (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        Task<Unit> IRequestHandler<DidChangeTextDocumentParams, Unit>.Handle
            (DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        Task<Unit> IRequestHandler<DidCloseTextDocumentParams, Unit>.Handle
            (DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        Task<Unit> IRequestHandler<DidOpenTextDocumentParams, Unit>.Handle
            (DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        Task<Unit> IRequestHandler<DidSaveTextDocumentParams, Unit>.Handle
            (DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }

        TextDocumentAttributes ITextDocumentIdentifier.GetTextDocumentAttributes(DocumentUri uri)
        {
            NotImplementedMethod(request, cancellationToken);
            return null;
        }
    }
}