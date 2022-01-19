using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ReniVSIX;

public sealed class ReniService : LanguageService
{
    readonly ValueCache<LanguagePreferences> PreferencesCache;

    public ReniService() => PreferencesCache = new(CreateLanguagePreferences);

    public override LanguagePreferences GetLanguagePreferences()
        => PreferencesCache.Value;

    public override IScanner GetScanner(IVsTextLines buffer) => DummyScanner.Instance;

    public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
        => new SourceWrapper(this, buffer);

    public override AuthoringScope ParseSource(ParseRequest request)
    {
        if(request.Reason == ParseReason.HighlightBraces)
            return new HighlightBracesScope(request);

        Dumpable.NotImplementedFunction(request);
        return default;
    }

    public override string GetFormatFilterList() => "Reni files (*.reni)\n*.reni\n";

    public override string Name => Constants.LanguageName;

    internal ReniVSIXPackage Package => (ReniVSIXPackage)Site;

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