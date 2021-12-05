using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ReniVSIX
{
    public sealed class ReniService : LanguageService
    {
        readonly ValueCache<LanguagePreferences> PreferencesCache;

        public ReniService() => PreferencesCache = new ValueCache<LanguagePreferences>(CreateLanguagePreferences);

        public override LanguagePreferences GetLanguagePreferences()
            => PreferencesCache.Value;

        public override IScanner GetScanner(IVsTextLines buffer) => DummyScanner.Instance;

        public ReniVSIXPackage Package
            => (ReniVSIXPackage) GetLanguagePreferences().GetSite();

        public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
            => new SourceWrapper(this, buffer);

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            Dumpable.NotImplementedFunction(req);
            return default;
        }

        public override string GetFormatFilterList() => "Reni files (*.reni)\n*.reni\n";

        public override string Name => "Reni";

        LanguagePreferences CreateLanguagePreferences()
            => new LanguagePreferences(Site, typeof(ReniService).GUID, Name)
            {
                EnableMatchBraces = true
                , EnableMatchBracesAtCaret = true
                , EnableShowMatchingBrace = true
                , EnableCommenting = true
                , EnableCodeSense = true
                , AutoOutlining = true
                , EnableFormatSelection = true
            };
    }
}