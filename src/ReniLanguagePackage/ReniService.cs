using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    [UsedImplicitly]
    public sealed class ReniService : LanguageService
    {
        readonly ValueCache<LanguagePreferences> _preferencesCache;

        public ReniService()
        {
            _preferencesCache = new ValueCache<LanguagePreferences>
                (CreateLanguagePreferences);
        }

        LanguagePreferences CreateLanguagePreferences()
            => new LanguagePreferences(Site, typeof(ReniService).GUID, Name)
            {
                EnableMatchBraces = true,
                EnableMatchBracesAtCaret = true,
                EnableShowMatchingBrace = true,
                EnableCommenting = true,
                EnableCodeSense = true
            };

        public override string Name => "Reni";

        public override LanguagePreferences GetLanguagePreferences()
            => _preferencesCache.Value;

        public override Colorizer GetColorizer(IVsTextLines buffer)
            => new ColorizerWrapper(this, buffer);

        public override IScanner GetScanner(IVsTextLines buffer) => new ScannerWrapper(buffer);

        public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource
            (ParseRequest request)
            => new AuthoringScopeWrapper(new ParseRequestWrapper(request));

        public override string GetFormatFilterList() => "Reni files (*.reni)\n*.reni\n";

        public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
            => new SourceWrapper(this, buffer);
    }
}