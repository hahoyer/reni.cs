using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    static class Extension
    {
        internal static ValueSyntax GetInfixSyntax
        (
            this ValueSyntax left,
            BinaryTree target,
            ValueSyntax right, Anchor frameItems
        )
            => left == null
                ? right == null
                    ? (ValueSyntax)new TerminalSyntax((ITerminal)target.TokenClass, target.Token, frameItems)
                    : new PrefixSyntax((IPrefix)target.TokenClass, right, target.Token, frameItems)
                : right == null
                    ? (ValueSyntax)new SuffixSyntax(left, (ISuffix)target.TokenClass, target.Token, frameItems)
                    : new InfixSyntax(left, (IInfix)target.TokenClass, right, target.Token, frameItems);

        internal static IStatementSyntax[] With(this IStatementSyntax[] statements, Anchor frameItems)
        {
            if(frameItems == null || !frameItems.Items.Any())
                return statements;

            if(frameItems.Items.Last().SourcePart < statements.First().SourcePart)
                return T(statements.First().With(frameItems)).Concat(statements.Skip(1)).ToArray();

            $@"{nameof(statements)}[{statements.Length}] = 
------------------
{statements.Select(statement=>statement.SourcePart.GetDumpAroundCurrent(20)).Stringify("\n------------------\n")}
------------------

------------------
{nameof(frameItems)}[{frameItems.Items.Length}] = 
------------------
{frameItems.Items.Select(item=>item.SourcePart.GetDumpAroundCurrent(20)).Stringify("\n------------------\n")}
------------------

"
            .Log();

            Dumpable.NotImplementedFunction(statements, frameItems);
            return default;
        }

        static TValue[] T<TValue>(params TValue[] value) => value;
    }

}