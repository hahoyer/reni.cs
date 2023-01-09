using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP;

sealed class DocumentFormattingHandlerWrapper : DocumentFormattingHandlerBase
{
    readonly Handler Handler;
    public DocumentFormattingHandlerWrapper(Handler handler) => Handler = handler;

    protected override DocumentFormattingRegistrationOptions CreateRegistrationOptions
        (DocumentFormattingCapability capability, ClientCapabilities clientCapabilities)
        => Handler.FormattingOptions;

    public override async Task<TextEditContainer> Handle
        (DocumentFormattingParams request, CancellationToken cancellationToken)
        => await Handler.Format(request);
}