using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    interface IDeclaratorTokenClass
    {
        Result<Declarator> Get(Syntax left, SourcePart token, Syntax right);
    }

    sealed class Declarator : DumpableObject
    {
        [EnableDump]
        IDeclarationTag[] Tags { get; }
        [EnableDump]
        Definable Target { get; }

        internal Declarator(IDeclarationTag[] tags, Definable target)
        {
            Tags = tags;
            Target = target;
        }

        internal Result<Statement> Statement(SourcePart token, Value right)
            => new Statement(Tags, Target, token, right);

        public Declarator WithName(Definable target)
        {
            if(Target == null)
                return new Declarator(Tags, target);

            NotImplementedMethod(target);
            return null;
        }
    }

    interface IDeclaratorTagProvider
    {
        Result<Declarator> Get(Syntax left, SourcePart token, Syntax right);
    }

    interface IDeclarationTag {}

    sealed class ExclamationBoxToken : DumpableObject,
        IType<Syntax>,
        ITokenClass,
        IDeclaratorTokenClass
    {
        Syntax Value { get; }

        internal ExclamationBoxToken(Syntax value) { Value = value; }

        Syntax IType<Syntax>.Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return Syntax.CreateSourceSyntax(left, this, token, Value);
        }

        string IType<Syntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";

        Result<Value> ITokenClass.GetValue(Syntax left, SourcePart token, Syntax right)
            => IssueId.InvalidExpression.Syntax(token, new EmptyList(token));

        Result<Declarator> IDeclaratorTokenClass.Get(Syntax left, SourcePart token, Syntax right)
        {
            if(left == null && right != null)
            {
                var exclamationProvider = right.TokenClass as IDeclaratorTagProvider;
                if(exclamationProvider != null)
                    return exclamationProvider
                        .Get(right.Left, right.Token.Characters, right.Right);
            }

            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}