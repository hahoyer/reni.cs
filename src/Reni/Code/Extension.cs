using Reni.Basics;
using Reni.Type;

namespace Reni.Code;

static class Extension
{
    internal static CodeBase ToSequence(this IEnumerable<CodeBase> x) 
        => x.Aggregate(CodeBase.Void, (code, result) => code + result);

    internal static CodeBase GetCode(this IContextReference reference)
        => new ReferenceCode(reference);

    internal static CodeBase GetCode(this BitsConst t, Size size) => new BitArray(size, t);
    internal static CodeBase GetCode(BitsConst t) => GetCode(t, t.Size);

    internal static CodeBase GetDumpPrintTextCode(this string dumpPrintText)
        => new DumpPrintText(dumpPrintText);

    internal static FiberItem GetRecursiveCall(this Size refsSize)
        => new RecursiveCallCandidate(refsSize);

    internal static CodeBase GetCode(this IEnumerable<CodeBase> data)
    {
        var allData = data
            .SelectMany(item => item.ToList())
            .ToArray();

        return List.Create(allData);
    }

    internal static Closures GetClosures(this CodeBase[] codeBases)
    {
        var closures = codeBases.Select(code => code.Closures).ToArray();
        return closures.Aggregate(Closures.GetVoid(), (r1, r2) => r1.Sequence(r2));
    }

    internal static CodeBase GetArgumentCode(this TypeBase type) => new Argument(type);
}