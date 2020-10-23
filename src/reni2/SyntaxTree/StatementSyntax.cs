using System;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    [Obsolete("", true)]
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
        protected override int LeftDirectChildCountKernel => 0;

        [DisableDump]
        protected override int DirectChildCountKernel => 1;

        protected override Syntax GetDirectChildKernel(int index)
            => index switch
            {
                0 => (Syntax)Content, _ => null
            };

        internal StatementSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }

        internal bool IsDefining
            (string name, bool publicOnly) => Content.Declarer?.IsDefining(name, publicOnly) ?? false;
    }
}