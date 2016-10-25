using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(DeclarationTokenFactory))]
    sealed class RightDeclarationParenthesis : RightParenthesisBase,
        IDeclaratorTagProvider,
        IBracketMatch<Syntax>

    {
        sealed class Matched : DumpableObject, IParserTokenType<Syntax>
        {
            Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
            {
                Tracer.Assert(right == null);
                return left;
            }

            string IParserTokenType<Syntax>.PrioTableId => "()";
        }

        public RightDeclarationParenthesis(int level)
            : base(level) { }

        Result<Declarator> IDeclaratorTagProvider.Get(Syntax syntax)
        {
            var bracketKernel = syntax.Left.GetBracketKernel(Level, syntax);
            var target = bracketKernel.Target;
            if(target != null)
            {
                var items = target
                    .Items
                    .Select(GetDeclarationTag)
                    .ToArray();

                var result = new Declarator
                (
                    items.Select(item => item.Target).Where(item => item != null).ToArray(),
                    null,
                    syntax.SourcePart
                );
                var issues = items.SelectMany(item => item.Issues).ToArray();
                return result.Issues(issues);
            }
            else
            {
                var issues = bracketKernel.Issues.plus(IssueId.MissingDeclarationTag.Create(syntax.SourcePart));
                return new Declarator(null, null,syntax.SourcePart).Issues(issues);
            }
        }

        static Result<IDeclarationTag> GetDeclarationTag(Syntax item)
        {
            var result = item.Option.DeclarationTag;
            if(result != null)
                return new Result<IDeclarationTag>(result);

            return new Result<IDeclarationTag>(null, IssueId.InvalidDeclarationTag.Create(item.SourcePart));
        }

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
    }
}