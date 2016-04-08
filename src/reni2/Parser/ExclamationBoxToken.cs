using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken : DumpableObject, IType<Syntax>, ITokenClass
    {
        Syntax Value { get; }

        internal ExclamationBoxToken(Syntax value) { Value = value; }

        Syntax IType<Syntax>.Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return Syntax.CreateSourceSyntax(left, this, token, Value);
        }

        static Checked<ExclamationSyntaxList> GetResult(OldSyntax left, IToken token, OldSyntax right)
        {
            var result = right.ExclamationSyntax(token.Characters);
            if(left == null)
                return result;

            var leftResult = left.Combine(result.Value);
            return new Checked<ExclamationSyntaxList>
                (leftResult.Value, leftResult.Issues.plus(result.Issues));
        }

        string IType<Syntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";

        Checked<Value> ITokenClass.GetValue(Syntax left, SourcePart token, Syntax right)
        {
            if(left == null && right != null)
            {
                var exclamationProvider = right.TokenClass as IExclamationTagProvider;
                if(exclamationProvider != null)
                {
                    var result = exclamationProvider.GetTags(right.Left, right.Token.Characters, right.Right);
                    return result.Value.ToCompiledSyntax.With(result.Issues);
                }
            }
            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    interface IExclamationTagProvider
    {
        Checked<ExclamationSyntaxList> GetTags(Syntax left, SourcePart token, Syntax right);
    }
}