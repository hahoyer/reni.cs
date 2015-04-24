﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
                EnableCodeSense = true,
                AutoOutlining = true,
                EnableFormatSelection = true
            };

        public override string Name => "Reni";

        public override LanguagePreferences GetLanguagePreferences()
            => _preferencesCache.Value;

        public override Colorizer GetColorizer(IVsTextLines buffer)
            => new ColorizerWrapper(this, buffer);

        public override IScanner GetScanner(IVsTextLines buffer) => new DummyScanner();

        public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource
            (ParseRequest request)
        {
            var wrapper = new ParseRequestWrapper(request);
            wrapper.Execute();
            return new AuthoringScopeWrapper();
        }

        public override string GetFormatFilterList() => "Reni files (*.reni)\n*.reni\n";

        public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
            => new SourceWrapper(this, buffer);
    }
}