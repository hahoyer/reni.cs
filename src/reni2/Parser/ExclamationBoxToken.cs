using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    interface IDeclaratorTokenClass
    {
        Result<Declarator> Get(Syntax syntax);
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

        internal Result<Statement> Statement(Result<Value> right, IDefaultScopeProvider container)
            => Parser.Statement.Create(Tags, Target, right, container);

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
        Result<Declarator> Get(Syntax syntax);
    }

    interface IDeclarationTag {}

    sealed class ExclamationBoxToken : DumpableObject,
        IParserTokenType<Syntax>,
        ITokenClass,
        IDeclaratorTokenClass
    {
        Syntax Value { get; }

        internal ExclamationBoxToken(Syntax value) { Value = value; }

        Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return Syntax.CreateSourceSyntax(left, this, token, Value);
        }

        string IParserTokenType<Syntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";

        Result<Declarator> IDeclaratorTokenClass.Get(Syntax syntax)
        {
            if(syntax.Left == null && syntax.Right != null)
            {
                var exclamationProvider = syntax.Right.TokenClass as IDeclaratorTagProvider;
                if(exclamationProvider != null)
                    return exclamationProvider
                        .Get(syntax.Right);
            }

            NotImplementedMethod(syntax);
            return null;
        }
    }
}