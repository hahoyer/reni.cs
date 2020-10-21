using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class StatementSyntax : Syntax
    {
        [EnableDumpExcept(null)]
        internal readonly IStatementSyntax Content;

        StatementSyntax(BinaryTree anchor, IStatementSyntax content)
            : base(anchor)
        {
            Content = content;
            AssertValid();
            StopByObjectIds(649);
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
                0 => (Syntax)Content, _ => null
            };

        internal StatementSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }

        internal static StatementSyntax[] Create(BinaryTree target, IStatementSyntax statement)
            => T(new StatementSyntax(target, statement));

        internal bool IsDefining
            (string name, bool publicOnly) => Content.Declarer?.IsDefining(name, publicOnly) ?? false;
    }
}