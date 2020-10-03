﻿using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class Declarer : DumpableObject
    {
        SourcePart Position { get; }
        [EnableDump]
        IDeclarationTag[] Tags { get; }
        [EnableDump]
        Definable Target { get; }

        internal Declarer(IDeclarationTag[] tags, Definable target, SourcePart position)
        {
            Position = position;
            Tags = tags;
            Target = target;
        }

        internal Result<Statement> Statement(Result<Syntax> right, IDefaultScopeProvider container)
            => Parser.Statement.Create(Tags, Target, right, container);

        public Result<Declarer> WithName(Definable target, SourcePart position)
        {
            var result = new Declarer(Tags, target, Position + position);
            if (Target == null)
                return result;

            return result.Issues(IssueId.InvalidDeclarationTag.Issue(Position));
        }

        public Declarer Combine(Declarer other)
        {
            Tracer.Assert(Target == null|| other.Target == null);
            return new Declarer(Tags.plus(other.Tags), Target ?? other.Target, Position + other.Position);
        }
    }

    interface IDeclarerTokenClass
    {
        Result<Declarer> Get(BinaryTree binaryTree);
    }

    interface IDeclarerTagProvider
    {
        Result<Declarer> Get(BinaryTree binaryTree);
    }

    interface IDeclarationTag {}
}