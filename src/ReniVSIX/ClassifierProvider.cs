using System.ComponentModel.Composition;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("reni")]
    class ClassifierProvider : IClassifierProvider
    {
        [UsedImplicitly]
        [Import]
        IClassificationTypeRegistryService ClassificationRegistry;

        IClassifier IClassifierProvider.GetClassifier(ITextBuffer buffer)
            => buffer.Properties.GetOrCreateSingletonProperty(()
                => new Classifier(buffer, ClassificationRegistry));
    }
}