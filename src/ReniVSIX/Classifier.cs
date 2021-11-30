using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using ReniUI;
using ReniUI.Classification;

namespace ReniVSIX
{
    class Classifier : DumpableObject, IClassifier, ITagger<IErrorTag>
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
            => CompilerBrowser
                .FromText(Buffer.CurrentSnapshot.GetText())
                .GetClassification(span.Start.Position, span.End.Position).ToArray()
                .Select(item => GetClassificationSpan(item, span)).ToArray()
                .ToList();

        IEnumerable<ITagSpan<IErrorTag>> ITagger<IErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans) 
            => spans
            .Select(GetTag)
            .ToArray();

        ITagSpan<IErrorTag> GetTag(SnapshotSpan span)
        {
            var item = CompilerBrowser
                .FromText(Buffer.CurrentSnapshot.GetText()).Locate(span.Start.Position);

            
            var snapshotSpan = new TagSpan<IErrorTag>(span,new 
                ErrorTag("syntax error", item));
            
            
            throw new NotImplementedException();
        }

        SourcePart GetSourcePart(SnapshotSpan span) 
            => (new Source(Buffer.CurrentSnapshot.GetText()) + span.Start.Position).Span(span.Length);

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        IClassificationType GetClassificationType(string s) => Registry.GetClassificationType(s);

        IClassificationType GetFormatTypeName(Item item)
        {
            if(item.IsNumber)
                return Types["number"];
            if(item.IsKeyword)
                return Types["keyword"];
            if(item.IsIdentifier)
                return Types["identifier"];
            if(item.IsWhiteSpace)
                return Types["text"];
            if(item.IsLineEnd)
                return Types["text"];
            if(item.IsError)
                return Types["syntax error"];
            if(item.IsText)
                return Types["string"];
            if(item.IsComment)
                return Types["comment"];
            if(item.IsLineComment)
                return Types["comment"];
            return Types["text"];
        }

        ClassificationSpan GetClassificationSpan(Item item, SnapshotSpan all)
        {
            var sourcePart = item.SourcePart;
            var snapshotSpan = new SnapshotSpan(all.Snapshot, new Span(sourcePart.Position, sourcePart.Length));
            var formatTypeName = GetFormatTypeName(item);
            formatTypeName.AssertIsNotNull();
            return new ClassificationSpan(snapshotSpan, formatTypeName);
        }
    }
}