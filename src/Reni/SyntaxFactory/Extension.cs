using hw.Scanner;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.SyntaxFactory;

static class Extension
{
    internal static ValueSyntax GetInfixSyntax
    (
        this ValueSyntax left,
        ITokenClass tokenClass, SourcePart token,
        ValueSyntax right,
        Anchor anchor
    )
        => left == null? right == null
                ? new TerminalSyntax((ITerminal)tokenClass, token, anchor)
                : new PrefixSyntax((IPrefix)tokenClass, right, token, anchor) :
            right == null? new SuffixSyntax(left, (ISuffix)tokenClass, token, anchor) :
            new InfixSyntax(left, (IInfix)tokenClass, right, token, anchor);

    internal static IStatementSyntax[] With(this IStatementSyntax[] statements, Anchor frameItems)
    {
        if(frameItems == null || !frameItems.Items.Any())
            return statements;

        if(frameItems.Items.Last().SourcePart < statements.First().SourcePart)
            return T(statements.First().With(frameItems)).Concat(statements.Skip(1)).ToArray();

        $@"{nameof(statements)}[{statements.Length}] = 
------------------
{statements.Select(statement => statement.SourcePart.GetDumpAroundCurrent(20)).Stringify("\n------------------\n")}
------------------

------------------
{nameof(frameItems)}[{frameItems.Items.Length}] = 
------------------
{frameItems.Items.Select(item => item.SourcePart.GetDumpAroundCurrent(20)).Stringify("\n------------------\n")}
------------------

"
            .Log();

        Dumpable.NotImplementedFunction(statements, frameItems);
        return default;
    }

    static TValue[] T<TValue>(params TValue[] value) => value;
}