using System.ComponentModel.Composition;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX
{
    [Export(typeof(IClassifierProvider))]
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("reni")]
    class ClassifierProvider : DumpableObject, IClassifierProvider, ITaggerProvider
    {
        [UsedImplicitly]
        [Import]
        IClassificationTypeRegistryService ClassificationRegistry;

        IClassifier IClassifierProvider.GetClassifier(ITextBuffer buffer)
            => buffer.Properties.GetOrCreateSingletonProperty(()
                => new Classifier(buffer, ClassificationRegistry));

        ITagger<T> ITaggerProvider.CreateTagger<T>(ITextBuffer buffer)
        {
            if(typeof(T).Is<IErrorTag>())
                return (ITagger<T>)new Classifier(buffer, ClassificationRegistry);

            return null;
        }
    }
}