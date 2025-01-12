using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using hw.Scanner;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace ReniLSP;

static class Extension
{
    public static string GetKey(this TextDocumentIdentifier target) => target.Uri.GetFileSystemPath();

    public static Range GetRange(this SourcePart token)
    {
        var range = token.TextPosition;
        return new(range.start.LineNumber, range.start.ColumnNumber, range.end.LineNumber
            , range.end.ColumnNumber);
    }

    public static bool? ToBoolean(this FormattingOptions option, string name)
    {
        if(option.ContainsKey(name) && option[name].IsBool)
            return option[name].Bool;
        return null;
    }

    public static int? ToInteger(this FormattingOptions option, string name)
    {
        if(option.ContainsKey(name) && option[name].IsInteger)
            return option[name].Integer;
        return null;
    }

    public static string ToString1(this ReadOnlySequence<byte> target) => Encoding.Default.GetString(target);
    public static string ToString1(this Span<byte> target) => Encoding.Default.GetString(target.ToArray());
}

public sealed class MyReader(string name, PipeReader target) : PipeReader
{
    readonly string Name = name;
    readonly PipeReader Target = target;
    public override string ToString() => Name;

    public override bool TryRead(out ReadResult result)
        => Target.TryRead(out result);

    public override async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = new())
    {
        var readResult = await Target.ReadAsync(cancellationToken);
        var text = readResult.Buffer.ToString1();
        @$"
{DateTime.Now.DynamicShortFormat(false)}:--------------- 
{text}
-------------------------------------

        ".Log();
        
        return readResult;
    }

    public override void AdvanceTo(SequencePosition consumed)
        => Target.AdvanceTo(consumed);

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        => Target.AdvanceTo(consumed, examined);

    public override void CancelPendingRead()
        => Target.CancelPendingRead();

    public override void Complete(Exception exception = null)
        => Target.Complete(exception);
}

public sealed class MyWriter(string name, PipeWriter target) : PipeWriter
{
    readonly string Name = name;
    readonly PipeWriter Target = target;
    public override string ToString() => Name;
    public override void Complete(Exception exception = null) => Target.Complete(exception);
    public override void CancelPendingFlush() => Target.CancelPendingFlush();

    public override async ValueTask<FlushResult> FlushAsync
        (CancellationToken cancellationToken = new()) => await Target.FlushAsync(cancellationToken);

    public override void Advance(int bytes) => Target.Advance(bytes);
    public override Memory<byte> GetMemory(int sizeHint = 0) => Target.GetMemory(sizeHint);

    public override Span<byte> GetSpan(int sizeHint = 0)
    {
        var span = Target.GetSpan(sizeHint);
        var text = span.ToString1();
        return span;
    }
}