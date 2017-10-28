using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class Declarator : DumpableObject
    {
        SourcePart Position { get; }
        [EnableDump]
        IDeclarationTag[] Tags { get; }
        [EnableDump]
        Definable Target { get; }

        internal Declarator(IDeclarationTag[] tags, Definable target, SourcePart position)
        {
            Position = position;
            Tags = tags;
            Target = target;
        }

        internal Result<Statement> Statement(Result<Value> right, IDefaultScopeProvider container)
            => Parser.Statement.Create(Tags, Target, right, container);

        public Result<Declarator> WithName(Definable target, SourcePart position)
        {
            var result = new Declarator(Tags, target, Position + position);
            if (Target == null)
                return result;

            return result.Issues(IssueId.InvalidDeclarationTag.Issue(Position));
        }

        public Declarator Combine(Declarator other)
        {
            Tracer.Assert(Target == null|| other.Target == null);
            return new Declarator(Tags.plus(other.Tags), Target ?? other.Target, Position + other.Position);
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