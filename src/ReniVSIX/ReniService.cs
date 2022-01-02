using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ReniUI;

namespace ReniVSIX
{
    public sealed class ReniService : LanguageService
    {
        readonly ValueCache<LanguagePreferences> PreferencesCache;

        public ReniService() => PreferencesCache = new ValueCache<LanguagePreferences>(CreateLanguagePreferences);

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

        public override string Name => "Reni";

        public ReniVSIXPackage Package
            => (ReniVSIXPackage)GetLanguagePreferences().GetSite();

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

    public class HighlightBracesScope : AuthoringScope
    {
        readonly CompilerBrowser Compiler;

        public HighlightBracesScope(ParseRequest request) 
            => Compiler = CompilerBrowser.FromText(request.Text, request.FileName);

        public override string GetDataTipText(int line, int column, out TextSpan span)
        {
            var (result, sourcePart) = Compiler.GetDataTipText(line, column);
            span = sourcePart.ToTextSpan();
            return result;
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason) => null;

        public override Methods GetMethods(int line, int col, string name) => null;

        public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            span = default;
            return null;
        }
    }
}