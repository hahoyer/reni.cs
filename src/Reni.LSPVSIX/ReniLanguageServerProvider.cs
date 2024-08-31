using System.Diagnostics;
using System.IO.Pipelines;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.LanguageServer;
using Nerdbank.Streams;
using ReniLSP;

namespace Reni.LSPVSIX;

#pragma warning disable VSEXTPREVIEW_LSP
[VisualStudioContribution]
[UsedImplicitly]
public sealed class ReniLanguageServerProvider : LanguageServerProvider
{
    [UsedImplicitly]
    Task ServerPipeTask;

    [VisualStudioContribution]
    internal static DocumentTypeConfiguration ReniDocumentType => new("reni")
    {
        FileExtensions = new[] { ".reni" }, BaseDocumentType = LanguageServerBaseDocumentType,
    };

    public ReniLanguageServerProvider
        (ExtensionCore container, VisualStudioExtensibility extensibilityObject, TraceSource traceSource)
        : base(container, extensibilityObject) { }

    public override LanguageServerProviderConfiguration LanguageServerProviderConfiguration =>
        new("%Reni Language Server%",
            new[]
            {
                DocumentFilter.FromDocumentType("reni"),
            });

    public override Task<IDuplexPipe> CreateServerConnectionAsync(CancellationToken token)
    {
        var (pipeToLSP, pipeToVS) = FullDuplexStream.CreatePair();
        var reader = pipeToLSP.UsePipeReader();
        var writer = pipeToLSP.UsePipeWriter();
        ServerPipeTask = Task.Run(() => MainContainer.RunServer(reader, writer), token);
        var duplexPipe = new DuplexPipe(pipeToVS.UsePipeReader(), pipeToVS.UsePipeWriter());

        return Task.FromResult<IDuplexPipe>(duplexPipe);
    }
}