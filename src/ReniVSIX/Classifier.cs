using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using ReniUI;
using ReniUI.Classification;

namespace ReniVSIX
{
    class Classifier : DumpableObject, IClassifier
    {
        readonly ITextBuffer Buffer;
        readonly IClassificationTypeRegistryService Registry;
        readonly FunctionCache<string, IClassificationType> Types;

        internal Classifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            Buffer = buffer;
            Registry = registry;
            Types = new FunctionCache<string, IClassificationType>(GetClassificationType);
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        IList<ClassificationSpan> IClassifier.GetClassificationSpans(SnapshotSpan span)
        {
            var items = CompilerBrowser.FromText(Buffer.CurrentSnapshot.GetText())
                .GetClassification(span.Start.Position, span.End.Position).ToArray();
            var spans = items
                .Select(item => CreateClassificationSpan(item, span)).ToArray();
            return spans.ToList();
        }

        IClassificationType GetClassificationType(string s) => Registry.GetClassificationType(s);

        IClassificationType GetFormatTypeName(Item item)
        {
            if(item.IsComment)
                return Types["comment"];
            if(item.IsLineComment)
                return Types["comment"];
            if(item.IsWhiteSpace)
                return Types["text"];
            if(item.IsLineEnd)
                return Types["text"];
            if(item.IsNumber)
                return Types["number"];
            if(item.IsText)
                return Types["string"];
            if(item.IsKeyword)
                return Types["keyword"];
            if(item.IsIdentifier)
                return Types["identifier"];
            return Types["text"];
        }

        ClassificationSpan CreateClassificationSpan(Item item, SnapshotSpan all)
        {
            var sourcePart = item.SourcePart;
            var snapshotSpan = new SnapshotSpan(all.Snapshot, new Span(sourcePart.Position, sourcePart.Length));
            var formatTypeName = GetFormatTypeName(item);
            formatTypeName.AssertIsNotNull();
            return new ClassificationSpan(snapshotSpan, formatTypeName);
        }
    }


    static class Extension { }
}