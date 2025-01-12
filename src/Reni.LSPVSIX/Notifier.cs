using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;

namespace Reni.LSPVSIX;

[Experimental("VSEXTPREVIEW_SETTINGS")]
[UsedImplicitly]
public sealed class Notifier : DumpableObject
{
    public Notifier(VisualStudioExtensibility extensibility)
        => Initialize(extensibility);

    internal static void Initialize(VisualStudioExtensibility extensibility)
        => extensibility.Settings().SubscribeAsync([SettingDefinitions.Reni], CancellationToken.None, OnChange);

    static void OnChange(SettingValues obj)
    {
        //ReniUI.Formatting.Configuration formatOptions = new();

        //foreach(var pair in obj)
        //    SettingDefinitions.Convert(pair, formatOptions);

        //NotImplementedFunction(obj);
    }
}