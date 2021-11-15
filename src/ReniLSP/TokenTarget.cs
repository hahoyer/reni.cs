using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;
using JetBrains.Annotations;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP
{
    [UsedImplicitly]
    sealed class TokenTarget : SemanticTokensHandlerBase
    {
        public TokenTarget() => Debugger.Launch();

        protected override SemanticTokensRegistrationOptions CreateRegistrationOptions
            (SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
            => GetSemanticTokensRegistrationOptions();

        protected override Task Tokenize
        (
            SemanticTokensBuilder builder
            , ITextDocumentIdentifierParams identifier
            , CancellationToken cancellationToken
        )
        {
            Dumpable.NotImplementedFunction(builder, identifier, cancellationToken);
            return Task.FromResult<SemanticTokens>(null);
        }

        protected override Task<SemanticTokensDocument> GetSemanticTokensDocument
            (ITextDocumentIdentifierParams @params, CancellationToken cancellationToken)
        {
            Dumpable.NotImplementedFunction(@params, cancellationToken);
            return Task.FromResult<SemanticTokensDocument>(null);
        }

        public override Task<SemanticTokens> Handle(SemanticTokensParams request, CancellationToken cancellationToken)
        {
            Dumpable.NotImplementedFunction(request, cancellationToken);
            return Task.FromResult<SemanticTokens>(null);
        }

        public override Task<SemanticTokensFullOrDelta> Handle
            (SemanticTokensDeltaParams request, CancellationToken cancellationToken)
        {
            Dumpable.NotImplementedFunction(request, cancellationToken);
            return Task.FromResult<SemanticTokensFullOrDelta>(null);
        }

        public override Task<SemanticTokens> Handle
            (SemanticTokensRangeParams request, CancellationToken cancellationToken)
        {
            Dumpable.NotImplementedFunction(request, cancellationToken);
            return Task.FromResult<SemanticTokens>(null);
        }

        static SemanticTokensRegistrationOptions GetSemanticTokensRegistrationOptions()
            => new()
            {
                DocumentSelector = DocumentSelector.ForLanguage("reni")
                , Legend = new()
                {
                    TokenModifiers = new("type")
                    , TokenTypes = new("local")
                }
                , Full = true
                , Range = true
            };
    }
}