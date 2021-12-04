using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using ReniUI;
using ReniUI.Classification;

namespace ReniVSIX
{
    class Classifier : DumpableObject, IClassifier, ITagger<IErrorTag>, ITagger<TextMarkerTag>
    {
        readonly ITextBuffer Buffer;
        readonly IClassificationTypeRegistryService Registry;
        readonly FunctionCache<string, IClassificationType> Types;

        readonly ValueCache<CompilerBrowser> CompilerCache;

        internal Classifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            Buffer = buffer;
            Registry = registry;
            Types = new FunctionCache<string, IClassificationType>(GetClassificationType);
            CompilerCache = new ValueCache<CompilerBrowser>(GetCompiler);
            Buffer.Changed += (sender, args) => CompilerCache.IsValid = false;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        IList<ClassificationSpan> IClassifier.GetClassificationSpans(SnapshotSpan span)
            => Compiler
                .GetClassification(span.Start.Position, span.End.Position)
                .Select(GetClassificationSpan)
                .ToList();

        IEnumerable<ITagSpan<IErrorTag>> ITagger<IErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
            => spans
                .SelectMany(span => Compiler.GetClassification(span.Start.Position, span.End.Position))
                .SelectMany(GetErrorTag)
                .ToArray();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        IEnumerable<ITagSpan<TextMarkerTag>> ITagger<TextMarkerTag>.GetTags(NormalizedSnapshotSpanCollection spans)
            => spans
                .SelectMany(span => Compiler.GetClassification(span.Start.Position, span.End.Position))
                .SelectMany(GetTextMarkerTag)
                .ToArray();

        CompilerBrowser Compiler => CompilerCache.Value;

        CompilerBrowser GetCompiler() => CompilerBrowser.FromText(Buffer.CurrentSnapshot.GetText());


        IEnumerable<ITagSpan<TextMarkerTag>> GetTextMarkerTag(Item item)
        {
            if(item.IsBrace)
                yield return new TagSpan<TextMarkerTag>(ToSpan(item.SourcePart), new TextMarkerTag("Brace"));
        }

        IEnumerable<ITagSpan<IErrorTag>> GetErrorTag(Item item)
        {
            if(item.IsError)
                yield return new TagSpan<IErrorTag>(ToSpan(item.SourcePart)
                    , new ErrorTag(PredefinedErrorTypeNames.SyntaxError, item.Issue.Message));
        }

        SourcePart GetSourcePart(SnapshotSpan span)
            => (new Source(Buffer.CurrentSnapshot.GetText()) + span.Start.Position).Span(span.Length);

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

        ClassificationSpan GetClassificationSpan(Item item)
        {
            var sourcePart = item.SourcePart;
            var snapshotSpan = ToSpan(sourcePart);
            var formatTypeName = GetFormatTypeName(item);
            formatTypeName.AssertIsNotNull();
            return new ClassificationSpan(snapshotSpan, formatTypeName);
        }

        SnapshotSpan ToSpan
            (SourcePart sourcePart)
            => new SnapshotSpan(Buffer.CurrentSnapshot, new Span(sourcePart.Position, sourcePart.Length));
    }
}