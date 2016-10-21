using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.Parser
{
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

        public Declarator Combine(Declarator other)
        {
            Tracer.Assert(Target == null|| other.Target == null);
            return new Declarator(Tags.plus(other.Tags), Target ?? other.Target);
        }
    }

    interface IDeclaratorTokenClass
    {
        Result<Declarator> Get(Syntax syntax);
    }

    interface IDeclaratorTagProvider
    {
        Result<Declarator> Get(Syntax syntax);
    }

    interface IDeclarationTag {}
}