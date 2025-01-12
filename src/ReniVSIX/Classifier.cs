using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using ReniUI.Classification;

namespace ReniVSIX;

sealed class Classifier : DumpableObject, IClassifier
{
    readonly IClassificationTypeRegistryService Registry;
    readonly FunctionCache<string, IClassificationType> Types;
    readonly BufferContainer Buffer;

    internal Classifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
    {
        Buffer = new(buffer);
        Registry = registry;
        Types = new(GetClassificationType);
    }

    event EventHandler<ClassificationChangedEventArgs> IClassifier.ClassificationChanged
    {
        add { }
        remove { }
    }

    IList<ClassificationSpan> IClassifier.GetClassificationSpans(SnapshotSpan span)
        => Buffer
            .Compiler
            .Locate(span.Start.Position, span.End.Position)
            .Select(GetClassificationSpan)
            .ToList();

    IClassificationType GetClassificationType(string s) => Registry.GetClassificationType(s);

    IClassificationType GetFormatTypeName(Item item)
    {
        if(item.IsPunctuation)
            return Types["punctuation"];
        if(item.IsNumber)
            return Types["number"];
        if(item.IsKeyword)
            return Types["keyword"];
        if(item.IsIdentifier)
            return Types["identifier"];
        if(item.IsSpace)
            return Types["text"];
        if(item.IsLineEnd)
            return Types["text"];
        if(item.IsText)
            return Types["string"];
        if(item.IsBrace)
            return Types["keyword"];
        if(item.IsComment)
            return Types["comment"];
        return Types["text"];
    }

    ClassificationSpan GetClassificationSpan(Item item)
    {
        //("class " + item.ShortDump()).Log();
        var sourcePart = item.SourcePart;
        var snapshotSpan = Buffer.ToSpan(sourcePart);
        var formatTypeName = GetFormatTypeName(item);
        formatTypeName.AssertIsNotNull();
        return new(snapshotSpan, formatTypeName);
    }
}