using hw.Helper;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ReniVSIX;

public sealed class ReniService : LanguageService
{
    readonly ValueCache<LanguagePreferences> PreferencesCache;

    internal ReniVSIXPackage Package => (ReniVSIXPackage)Site;

    public ReniService() => PreferencesCache = new(CreateLanguagePreferences);

    public override LanguagePreferences GetLanguagePreferences()
        => PreferencesCache.Value;

    public override IScanner GetScanner(IVsTextLines buffer) => null;

    public override Source CreateSource(IVsTextLines buffer) => null;

    public override AuthoringScope ParseSource(ParseRequest request) => null;

    public override string GetFormatFilterList() => "Reni files (*.reni)\n*.reni\n";

    public override string Name => Constants.LanguageName;

    LanguagePreferences CreateLanguagePreferences()
        => new(Site, typeof(ReniService).GUID, Name)
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