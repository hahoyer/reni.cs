using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    sealed class StatementSyntax : Syntax
    {
        [EnableDumpExcept(null)]
        internal readonly IStatementSyntax Content;

        StatementSyntax(BinaryTree anchor, IStatementSyntax content)
            : base(anchor)
        {
            Content = content;
            Tracer.Assert(anchor != null);

            AssertValid();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

        [DisableDump]
        internal override int LeftDirectChildCount => 0;

        [DisableDump]
        protected override int DirectChildCount => 1;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => (Syntax)Content
                , _ => null
            };

        internal StatementSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }

        internal static Result<StatementSyntax[]> Create(BinaryTree target, IStatementSyntax statement)
            => T(new StatementSyntax(target, statement));

        internal bool IsDefining(string name, bool publicOnly) => Content.Declarer?.IsDefining(name, publicOnly) ?? false;
    }
}