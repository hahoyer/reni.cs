using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using ReniUI.Classification;

namespace ReniVSIX
{
    class Tagger : BufferContainer, ITagger<IErrorTag>, ITagger<TextMarkerTag>
    {
        readonly ITextView View;
        SnapshotPoint? Caret;

        internal Tagger(ITextView view, ITextBuffer buffer)
            : base(buffer)
        {
            View = view;
            View.Caret.PositionChanged += (sender, args) => UpdateAtCaretPosition(args.NewPosition);
            View.LayoutChanged += (sender, args) =>
            {
                if(args.NewSnapshot != args.OldSnapshot)
                    UpdateAtCaretPosition(View.Caret.Position);
            };
        }

        IEnumerable<ITagSpan<IErrorTag>> ITagger<IErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
            => spans
                .SelectMany(span => Compiler.Locate(span.Start.Position, span.End.Position))
                .SelectMany(GetErrorTag)
                .ToArray();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        IEnumerable<ITagSpan<TextMarkerTag>> ITagger<TextMarkerTag>.GetTags(NormalizedSnapshotSpanCollection spans)
            => spans
                .SelectMany(GetTextMarkerTag)
                .ToArray();

        void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            Caret = caretPosition.Point.GetPoint(Buffer, caretPosition.Affinity);
            if(Caret == null)
                return;

            var tempEvent = TagsChanged;
            if(tempEvent != null)
                tempEvent(this
                    , new SnapshotSpanEventArgs(new SnapshotSpan(Buffer.CurrentSnapshot, 0
                        , Buffer.CurrentSnapshot.Length)));
        }


        IEnumerable<ITagSpan<IErrorTag>> GetErrorTag(Item item)
        {
            if(item.IsError)
            {
                ("error " + item.ShortDump()).Log();
                yield return new TagSpan<IErrorTag>(ToSpan(item.SourcePart)
                    , new ErrorTag(PredefinedErrorTypeNames.SyntaxError, item.Issue.Message));
            }
        }

        IEnumerable<ITagSpan<TextMarkerTag>> GetTextMarkerTag(SnapshotSpan span)
        {
            foreach(var tagSpan in TagSpans(span))
                yield return tagSpan;
        }

        IEnumerable<TagSpan<TextMarkerTag>> TagSpans(SnapshotSpan snapshotSpan)
        {
            if(Caret == null || Caret.Value.Position >= Caret.Value.Snapshot.Length)
                yield break;

            //hold on to a snapshot of the current character
            var caret = snapshotSpan.Snapshot == Caret.Value.Snapshot
                ? Caret.Value
                : Caret.Value.TranslateTo(snapshotSpan.Snapshot, PointTrackingMode.Positive);

            var item = Compiler.Locate(caret.Position);
            var belongingItems = item?.Belonging;
            if(belongingItems == null)
                yield break;

            yield return new TagSpan<TextMarkerTag>(ToSpan(item.SourcePart), new TextMarkerTag("Brace"));
            foreach(var belongingItem in belongingItems)
                yield return new TagSpan<TextMarkerTag>(ToSpan(belongingItem.SourcePart), new TextMarkerTag("blue"));
        }
    }
}