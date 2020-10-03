using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class Declarer : DumpableObject
    {
        [EnableDump]
        internal IDeclarationTag[] Tags { get; }

        [EnableDump]
        internal Definable Target { get; }

        [EnableDump]
        readonly BinaryTree BinaryTree;

        internal Declarer(IDeclarationTag[] tags, Definable target, BinaryTree binaryTree)
        {
            BinaryTree = binaryTree;
            Tags = tags ?? new IDeclarationTag[0];
            Target = target;
        }

        internal Result<Statement> Statement(Result<Syntax> right, IDefaultScopeProvider container)
            => Parser.Statement.Create(this, right, container);

        public Result<Declarer> WithName(Definable target, BinaryTree binaryTree)
        {


            NotImplementedMethod(target, binaryTree);

            var result = new Declarer(Tags, target, binaryTree);
            if(Target == null)
                return result;

            return result.Issues(IssueId.InvalidDeclarationTag.Issue(binaryTree.SourcePart));
        }

        public Declarer Combine(Declarer other)
        {
            NotImplementedMethod(other);

            Tracer.Assert(Target == null || other.Target == null);
            return new Declarer(Tags.plus(other.Tags), Target ?? other.Target, other.BinaryTree);
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

    interface IDeclarationTag { }
}