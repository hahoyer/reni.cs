using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
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
        readonly BinaryTree[] BinaryTrees;

        internal Declarer(IDeclarationTag[] tags, Definable target, BinaryTree[] binaryTrees)
        {
            BinaryTrees = binaryTrees;
            Tags = tags ?? new IDeclarationTag[0];
            Tracer.ConditionalBreak(BinaryTrees == null && Tags.Length ==2);
            Target = target;
        }

        internal Result<Statement> Statement(Result<ValueSyntax> right, IDefaultScopeProvider container)
            => Parser.Statement.Create(this, right, container);

        public Result<Declarer> WithName(Definable target, BinaryTree binaryTree)
        {
            var combinedBinaryTree = T(T(binaryTree),BinaryTrees).Combine();
            var result = new Declarer(Tags, target, combinedBinaryTree);
            if(Target == null)
                return result;
            var combinedSourcePart = combinedBinaryTree
                .Select(b => b.SourcePart)
                .Aggregate();

            return result.AddIssues(IssueId.InvalidDeclarationTag.Issue(combinedSourcePart));
        }

        public Declarer Combine(Declarer other)
        {
            Tracer.Assert(Target == null || other.Target == null);
            return new Declarer
            (
                Tags.plus(other.Tags),                                
                Target ?? other.Target,
                T(other.BinaryTrees,BinaryTrees).Combine()
            );
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