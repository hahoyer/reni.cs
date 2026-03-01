using Reni.Basics;
using Reni.Type;

namespace Reni.Code;

static class Extension
{
    extension(IEnumerable<CodeBase> x)
    {
        internal CodeBase ToSequence()
            => x.Aggregate(CodeBase.Void, (code, result) => code + result);
    }

    extension(IContextReference reference)
    {
        internal CodeBase Code => new ReferenceCode(reference);
    }

    extension(IEnumerable<CodeBase> data)
    {
        internal CodeBase Code
        {
            get
            {
                var allData = data
                    .SelectMany(item => item.ToList())
                    .ToArray();

                return List.Create(allData);
            }
        }

        internal Closures Closures
            => data
                .Select(code => code.Closures)
                .Aggregate(Closures.GetVoid(), (r1, r2) => r1.Sequence(r2));
    }

    extension(BitsConst t)
    {
        internal CodeBase Code => t.GetCode(t.Size);
        internal CodeBase GetCode(Size size) => new BitArray(size, t);
    }

    extension(TypeBase type)
    {
        internal CodeBase ArgumentCode => new Argument(type);
    }

    extension(Size refsSize)
    {
        internal FiberItem RecursiveCall => new RecursiveCallCandidate(refsSize);
    }

    extension(string dumpPrintText)
    {
        internal CodeBase DumpPrintTextCode => new DumpPrintText(dumpPrintText);
    }
}
