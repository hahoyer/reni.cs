using System.Collections.Generic;
using System.ComponentModel.Composition;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX;

[Export(typeof(ICompletionSourceProvider))]
[Export(typeof(IClassifierProvider))]
[Export(typeof(IViewTaggerProvider))]
[TagType(typeof(IErrorTag))]
[TagType(typeof(ITextMarkerTag))]
[Export(typeof(IIntellisenseControllerProvider))]
[Order(Before = "default")]
[Name(Constants.LanguageName + " language services")]
[ContentType(Constants.LanguageName)]
class ClassifierProvider
    : DumpableObject
        , IClassifierProvider
        , IViewTaggerProvider
        , ICompletionSourceProvider
        , IIntellisenseControllerProvider
{
    [Import]
    public IEditorOptionsFactoryService EditorOptions { get; set; }

    [UsedImplicitly]
    [Import]
    IClassificationTypeRegistryService ClassificationRegistry { get; set; }

    IClassifier IClassifierProvider.GetClassifier(ITextBuffer buffer)
    {
        Main.Instance.GetOptions(() => EditorOptions.GetOptions(buffer));
        return buffer
            .Properties
            .GetOrCreateSingletonProperty(() => new Classifier(buffer, ClassificationRegistry));
    }

    ICompletionSource ICompletionSourceProvider.TryCreateCompletionSource(ITextBuffer textBuffer)
        => new CompletionSource(textBuffer);

    IIntellisenseController
        IIntellisenseControllerProvider.TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> buffers)
        => new IntellisenseController(textView, buffers);

    ITagger<T> IViewTaggerProvider.CreateTagger<T>(ITextView view, ITextBuffer buffer)
        => new Tagger(view, buffer) as ITagger<T>;
}