using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Runtime;
using Reni.Struct;

namespace Reni.Code;

sealed class CSharpGenerator : DumpableObject, IVisitor
{
    readonly int TemporaryByteCount;
    readonly List<string> DataCache = new();
    int IndentLevel;

    public string Data
    {
        get
        {
            var start = $"\nvar data = Data.Create({TemporaryByteCount})";
            return DataCache
                    .Aggregate(start, (x, y) => x + ";\n" + y)
                + ";\n";
        }
    }

    static int RefBytes => DataHandler.RefBytes;

    public CSharpGenerator(int temporaryByteCount) => TemporaryByteCount = temporaryByteCount;

    void IVisitor.ArrayGetter(Size elementSize, Size indexSize)
        => AddCode($"data.ArrayGetter({elementSize.SaveByteCount},{indexSize.SaveByteCount})");

    void IVisitor.ArraySetter(Size elementSize, Size indexSize)
        => AddCode($"data.ArraySetter({elementSize.SaveByteCount},{indexSize.SaveByteCount})");

    void IVisitor.Assign(Size targetSize)
        => AddCode($"data.Assign({targetSize.SaveByteCount})");

    void IVisitor.BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
    {
        var sizeBytes = size.SaveByteCount;
        var leftBytes = leftSize.SaveByteCount;
        var rightBytes = rightSize.SaveByteCount;
        AddCode($"data.{opToken}(sizeBytes:{sizeBytes}, leftBytes:{leftBytes}, rightBytes:{rightBytes})");
    }

    void IVisitor.BitArrayPrefixOp(string operation, Size size, Size argSize)
    {
        var sizeBytes = size.SaveByteCount;
        var argBytes = argSize.SaveByteCount;
        AddCode(sizeBytes == argBytes
            ? $"data.{operation}Prefix(bytes:{sizeBytes})"
            : $"data.{operation}Prefix(sizeBytes:{sizeBytes}, argBytes:{argBytes})");
    }

    void IVisitor.BitCast(Size size, Size targetSize, Size significantSize)
        => AddCode(
            $"data.Push(data.Pull({targetSize.SaveByteCount}).BitCast({significantSize.ToInt()}).BitCast({size.ToInt()}))"
        );

    void IVisitor.BitsArray(Size size, BitsConst data)
        => AddCode($"data.SizedPush({size.ByteCount}{data.ByteSequence()})");

    void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
        => AddCode($"data.Push({Generator.FunctionName(functionId)}(data.Pull({argsAndRefsSize.SaveByteCount})))");

    void IVisitor.DePointer(Size size, Size dataSize)
        => AddCode
        (
            $"data.Push(data.Pull({RefBytes}).DePointer({dataSize.ByteCount}){BitCast(size, dataSize)})"
        );


    void IVisitor.Drop(Size beforeSize, Size afterSize)
        => AddCode(afterSize.IsZero
            ? $"data.Drop({beforeSize.ByteCount})"
            : $"data.Drop({beforeSize.ByteCount}, {afterSize.ByteCount})");

    void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
    {
        fiberHead.Visit(this);
        foreach(var fiberItem in fiberItems)
            fiberItem.Visit(this);
    }

    void IVisitor.List(CodeBase[] data)
    {
        foreach(var codeBase in data)
            codeBase.Visit(this);
    }

    void IVisitor.PrintNumber(Size leftSize, Size rightSize)
        => AddCode($"data.Pull({leftSize.SaveByteCount}).PrintNumber()");

    void IVisitor.PrintText(string dumpPrintText)
        => AddCode($"Data.PrintText({dumpPrintText.Quote()})");

    void IVisitor.PrintText(Size leftSize, Size itemSize)
        => AddCode
        (
            $"data.Pull({leftSize.SaveByteCount}).PrintText({itemSize.SaveByteCount})"
        );

    void IVisitor.RecursiveCall() => AddCode("goto Start");
    void IVisitor.RecursiveCallCandidate() => throw new UnexpectedRecursiveCallCandidate();

    void IVisitor.ReferencePlus(Size size)
        => AddCode("data.PointerPlus({0})", size.SaveByteCount);

    void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
    {
        AddCode($"if({PullBool(condSize.ByteCount)})\n{{{{");
        Indent();
        thenCode.Visit(this);
        BackIndent();
        AddCode("}}\nelse\n{{");
        Indent();
        elseCode.Visit(this);
        BackIndent();
        AddCode("}}");
    }

    void IVisitor.TopData(Size offset, Size size, Size dataSize)
        => AddCode
        (
            $"data.Push(data.Get({dataSize.ByteCount}, {offset.SaveByteCount}){BitCast(size, dataSize)})"
        );

    void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
        => AddCode
        (
            $"data.Push(frame.Get({dataSize.ByteCount}, {offset.SaveByteCount}){BitCast(size, dataSize)})"
        );

    void IVisitor.TopFrameRef(Size offset)
        => AddCode($"data.Push(frame.Pointer({offset.SaveByteCount}))");

    void IVisitor.TopRef(Size offset)
        => AddCode($"data.Push(data.Pointer({offset.SaveByteCount}))");

    [StringFormatMethod("pattern")]
    void AddCode(string pattern, params object[] data)
    {
        var c = string.Format(pattern, data);
        DataCache.Add("    ".Repeat(IndentLevel) + c);
    }

    static string BitCast(Size size, Size dataSize)
        => size == dataSize? "" : $".BitCast({dataSize.ToInt()}).BitCast({size.ToInt()})";

    static string PullBool(int byteCount)
        => byteCount == 1
            ? "data.Pull(1).GetBytes()[0] != 0"
            : $"data.Pull({byteCount}).IsNotNull()";

    void BackIndent() => IndentLevel--;
    void Indent() => IndentLevel++;

    internal static string GenerateCSharpStatements(CodeBase codeBase)
    {
        var generator = new CSharpGenerator(codeBase.TemporarySize.SaveByteCount);
        try
        {
            codeBase.Visit(generator);
        }
        catch(UnexpectedContextReference e)
        {
            Tracer.AssertionFailed("", () => e.Message);
        }

        return generator.Data;
    }
}