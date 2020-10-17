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
            
            AssertValid();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Binary?.Token.Characters.GetDumpAroundCurrent(5);

        [DisableDump]
        internal override int LeftDirectChildCount => 0;

        [DisableDump]
        protected override int DirectChildCount => 1;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 =>(Syntax) Content
                , _ => null
            };

        internal StatementSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }

        public static Result<StatementSyntax[]> Create(BinaryTree target, IStatementSyntax statement) 
            => T(new StatementSyntax(target, statement));
    }
}