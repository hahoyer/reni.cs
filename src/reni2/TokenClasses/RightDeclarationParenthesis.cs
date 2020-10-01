﻿using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(DeclarationTokenFactory))]
    sealed class RightDeclarationParenthesis
        : RightParenthesisBase,
            IDeclarerTagProvider,
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
            : base(level) {}

        [Obsolete("",true)]
        Result<Declarer> IDeclarerTagProvider.Get(Syntax syntax)
        {
            var bracketKernel = syntax.Left.GetBracketKernel(Level, syntax);
            var target = bracketKernel.Target;
            if(target != null)
            {
                var items = target
                    .Option
                    .Items
                    .Select(GetDeclarationTag)
                    .ToArray();

                var result = new Declarer
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
                var issues = bracketKernel.Issues.plus(IssueId.MissingDeclarationTag.Issue(syntax.SourcePart));
                return new Declarer(null, null, syntax.SourcePart).Issues(issues);
            }
        }

        [Obsolete("",true)]
        static Result<IDeclarationTag> GetDeclarationTag(Syntax item)
        {
            var result = item.Option.DeclarationTag;
            if(result != null)
                return new Result<IDeclarationTag>(result);

            return new Result<IDeclarationTag>(null, IssueId.InvalidDeclarationTag.Issue(item.SourcePart));
        }

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new Matched();
    }
}