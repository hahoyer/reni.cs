using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using ReniUI;
using ReniUI.Classification;

namespace ReniVSIX
{
    class Classifier : DumpableObject, IClassifier
    {
        readonly IClassificationTypeRegistryService Registry;
        readonly FunctionCache<string, IClassificationType> Types;

        [UsedImplicitly]
        [Import]
        IClassificationTypeRegistryService ClassificationRegistry;

        internal Classifier(IClassificationTypeRegistryService registry)
        {
            Registry = registry;
            Types = new FunctionCache<string, IClassificationType>(registry.GetClassificationType);
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        IList<ClassificationSpan> IClassifier.GetClassificationSpans(SnapshotSpan span)
        {
            return Enumerable.Empty<ClassificationSpan>().ToList();
            var items = CompilerBrowser.FromText(span.GetText())
                .GetClassification(span.Start.Position, span.End.Position).ToArray();
            var spans = items
                .Select(item => CreateClassificationSpan(item, span)).ToArray();
            return spans.ToList();
        }

        IClassificationType GetFormatTypeName(Item item)
        {
            if(item.IsError)
                return Types["error"];
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
                return Types["text"];
            return Types["text"];
        }

        ClassificationSpan CreateClassificationSpan(Item item, SnapshotSpan all)
        {
            var snapshotSpan = new SnapshotSpan(all.Snapshot, new Span(all.Start, all.Length));
            var formatTypeName = GetFormatTypeName(item);
            formatTypeName.AssertIsNotNull();
            return new ClassificationSpan(snapshotSpan, formatTypeName);
        }
    }


    static class Extension
    {
    }
}