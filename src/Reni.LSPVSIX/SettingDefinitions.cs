using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;

#pragma warning disable VSEXTPREVIEW_SETTINGS // The settings API is currently in preview and marked as experimental
namespace Reni.LSPVSIX;

public static class SettingDefinitions
{
    const string DisableFeature = "";

    [VisualStudioContribution]
    public static SettingCategory Reni { get; } = new("Reni", "Reni");

    [VisualStudioContribution]
    public static SettingCategory Formatting { get; } = new("formatting", "formatting", Reni);

    [VisualStudioContribution]
    public static SettingCategory List { get; }
        = new("List", "List", Formatting) { Description = "Use of line breaks." };

    [VisualStudioContribution]
    public static Setting.Boolean LineBreaksAtComplexDeclaration { get; }
        = new("LineBreaksAtComplexDeclaration", "Use line breaks at complex declaration", List, false)
        {
            Description = "If set, list with tagged names or values that are not simple terminals will be splitted."
        };

    [VisualStudioContribution]
    public static Setting.Boolean AdditionalLineBreaksForMultilineItems { get; }
        = new("AdditionalLineBreaksForMultilineItems",
            "Add additional line breaks for multiline items",
            List, true)
        {
            Description = "If set, multiline list elements will force an empty line before and after.",
        };

    [VisualStudioContribution]
    public static Setting.String MaxLineLength { get; }
        = new("MaxLineLength", "maximum lien length", Formatting, DisableFeature)
        {
            Description = "Specify the number of maximal characters per line. Leave empty to disable.",
        };


    [VisualStudioContribution]
    public static Setting.String EmptyLineLimit { get; }
        = new("EmptyLineLimit", "Empty line limit", Formatting, DisableFeature)
        {
            Description = "Specify the number of maximal line breaks between to lines of code. Leave empty to disable.",
        };

    [VisualStudioContribution]
    public static Setting.Boolean LineBreaksBeforeListToken { get; }
        = new("LineBreaksBeforeListToken",
            "Use line break before list token",
            List, false)
        {
            Description = "If set, line breaks will be used before list token.",
        };

    public static int? ToOptionalInt(this ISettingValue target)
    {
        var value = ((SettingValue<string>)target).Value;
        return value == DisableFeature? default(int?) : int.Parse(value);
    }

    public static bool? ToOptionalBool(this ISettingValue target)
    {
        var value = ((SettingValue<string>)target).Value;
        return value == DisableFeature? default : bool.Parse(value);
    }

    internal static void Convert
    (
        KeyValuePair<SettingIdentifier, ISettingValue> pair
        , ReniUI.Formatting.Configuration formatOptions
    )
    {
        if(LineBreaksBeforeListToken.TrySetValue(ref formatOptions.LineBreaksBeforeListToken, pair.Value))
            return;

        if(LineBreaksAtComplexDeclaration.TrySetValue(ref formatOptions.LineBreaksAtComplexDeclaration, pair.Value))
            return;

        if(EmptyLineLimit.TrySetValue(ref formatOptions.EmptyLineLimit, pair.Value))
            return;

        if(MaxLineLength.TrySetValue(ref formatOptions.MaxLineLength, pair.Value))
            return;

        if(AdditionalLineBreaksForMultilineItems.TrySetValue(ref formatOptions.AdditionalLineBreaksForMultilineItems
               , pair.Value))
            return;

        if(pair.Key.ToString().EndsWith("LineBreakAtEndOfText"))
            return;

        Dumpable.NotImplementedFunction(pair.Key.ToString(), formatOptions);
    }

    static bool Is(this Setting setting, ISettingValue target)
        => string.Equals(target.SettingIdentifier.ToString(), setting.FullId, StringComparison.OrdinalIgnoreCase);

    static bool TrySetValue(this Setting.String setting, ref int? target, ISettingValue settingValue)
    {
        if(!setting.Is(settingValue))
            return false;
        target = settingValue.ToOptionalInt();
        return true;
    }

    static bool TrySetValue(this Setting.Boolean setting, ref bool target, ISettingValue settingValue)
    {
        if(!setting.Is(settingValue))
            return false;
        target = settingValue.Value<bool>();
        return true;
    }
}