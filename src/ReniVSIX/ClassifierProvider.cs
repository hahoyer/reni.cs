using System.ComponentModel.Composition;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX;

[Export(typeof(IClassifierProvider))]
[Export(typeof(IViewTaggerProvider))]
[TagType(typeof(IErrorTag))]
[TagType(typeof(ITextMarkerTag))]
[ContentType(Constants.LanguageName)]
class ClassifierProvider : DumpableObject, IClassifierProvider, IViewTaggerProvider
{
    [Import]
    public IEditorOptionsFactoryService EditorOptionsCache;

    [UsedImplicitly]
    [Import]
    IClassificationTypeRegistryService ClassificationRegistry;

    IClassifier IClassifierProvider.GetClassifier(ITextBuffer buffer)
    {
        Main.Instance.GetOptions(() => EditorOptionsCache.GetOptions(buffer));
        return buffer
            .Properties
            .GetOrCreateSingletonProperty(() => new Classifier(buffer, ClassificationRegistry));
    }

    ITagger<T> IViewTaggerProvider.CreateTagger<T>(ITextView view, ITextBuffer buffer)
        => new Tagger(view, buffer) as ITagger<T>;
}