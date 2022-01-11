using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;

namespace ReniVSIX;

public sealed class ConfigurationProperties : DialogPage
{
    [Category("List")]
    [Description(
        "If set, multiline list elements " +
        "will force an empty line before and after.")]
    [UsedImplicitly]
    public bool AdditionalLineBreaksForMultilineItems { get; set; }

    [Description(
        "If set to true, a linebreak is ensured at the end of text.\n" +
        "If set to false, linebreaks at the end of text are removed if possible.")]
    [UsedImplicitly]
    public bool? LineBreakAtEndOfText { get; set; }

    [Description(
        "If set, tries to constraint line length until this value by inserting" +
        " line breaks at suitable positions.")]
    [UsedImplicitly]
    public int? MaxLineLength { get; set; }

    [Description(
        "If set, delimits the number of consecutive line breaks to this value.")]
    [UsedImplicitly]
    public int? EmptyLineLimit { get; set; }

    [Category("List")]
    [Description(
        "If set, linebreaks will be used before list token.")]
    [UsedImplicitly]
    public bool LineBreaksBeforeListToken { get; set; }
}

public sealed class ConfigurationProperties1 : DialogPage
{
    [Category("List")]
    [Description(
        "If set, multiline list elements " +
        "will force an empty line before and after.")]
    [UsedImplicitly]
    public bool AdditionalLineBreaksForMultilineItems { get; set; }

    [Description(
        "If set to true, a linebreak is ensured at the end of text.\n" +
        "If set to false, linebreaks at the end of text are removed if possible.")]
    [UsedImplicitly]
    public bool? LineBreakAtEndOfText { get; set; }

    [Description(
        "If set, tries to constraint line length until this value by inserting" +
        " line breaks at suitable positions.")]
    [UsedImplicitly]
    public int? MaxLineLength { get; set; }

    [Description(
        "If set, delimits the number of consecutive line breaks to this value.")]
    [UsedImplicitly]
    public int? EmptyLineLimit { get; set; }

    [Category("List")]
    [Description(
        "If set, linebreaks will be used before list token.")]
    [UsedImplicitly]
    public bool LineBreaksBeforeListToken { get; set; }
}