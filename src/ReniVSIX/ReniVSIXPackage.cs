using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using ReniUI.Formatting;

namespace ReniVSIX;

[ProvideService(typeof(ReniService), ServiceName = "Reni Language Service")]
[ProvideLanguageService(typeof(ReniService), Constants.LanguageName, 106)]
[ProvideLanguageExtension(typeof(ReniService), ".reni")]
[UsedImplicitly]
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(Constants.PackageGuidString)]
[ProvideLanguageEditorOptionPage(typeof(ConfigurationProperties), Constants.LanguageName, "Formatting", "", "100")]
public sealed class ReniVSIXPackage : AsyncPackage
{
    protected override async Task InitializeAsync
        (CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        Main.Instance.RegisterPackage(this);
    }

    internal Configuration GetFormattingConfiguration()
    {
        var properties = (ConfigurationProperties)GetDialogPage(typeof(ConfigurationProperties));
        var editorOptions = Main.Instance.EditorOptions;
        var indentCount = (int)editorOptions.GetOptionValue("Tabs/IndentSize");

        return new()
        {
            MaxLineLength = properties.MaxLineLength //
            , EmptyLineLimit = properties.EmptyLineLimit
            , AdditionalLineBreaksForMultilineItems = properties.AdditionalLineBreaksForMultilineItems
            , LineBreakAtEndOfText = properties.LineBreakAtEndOfText
            , LineBreaksBeforeListToken = properties.LineBreaksBeforeListToken
            , LineBreaksAtComplexDeclaration = properties.LineBreaksAtComplexDeclaration
            , IndentCount = indentCount
        };
    }

    [UsedImplicitly]
    static EditorOptionDefinition[] Filter(IEditorOptions options)
        //var o1 = Filter(editorOptions);
        //var g1 = Filter(editorOptions.GlobalOptions);
        => options
            .SupportedOptions
            //.Where(item => item.DefaultValue is int && (int)item.DefaultValue == 4)
            .Where(item => item.Name.Contains("BeforeList"))
            .ToArray();
}

