using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    class DeclarationSyntax : Syntax, IStatementSyntax
    {
        [EnableDumpExcept(null)]
        internal readonly DeclarerSyntax Declarer;

        [EnableDumpExcept(null)]
        internal readonly ValueSyntax Value;

        DeclarationSyntax
            (DeclarerSyntax declarer, BinaryTree anchor, ValueSyntax value, FrameItemContainer frameItems = null)
            : base(anchor, frameItems: frameItems ?? FrameItemContainer.Create())
        {
            Declarer = declarer;
            Value = value;

            Anchor.AssertIsNotNull();
            Declarer.AssertIsNotNull();
            Value.AssertIsNotNull();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor.Token.Characters.GetDumpAroundCurrent(5);

        [DisableDump]
        internal string NameOrNull => Declarer.Name?.Value;

        [DisableDump]
        internal bool IsConverterSyntax => Declarer.IsConverterSyntax;

        [DisableDump]
        internal bool IsMutableSyntax => Declarer.IsMutableSyntax;

        [DisableDump]
        protected override int LeftDirectChildCountKernel => Declarer.DirectChildCount;

        [DisableDump]
        protected override int DirectChildCountKernel => LeftDirectChildCount + 1;

        [DisableDump]
        DeclarerSyntax IStatementSyntax.Declarer => Declarer;

        SourcePart IStatementSyntax.SourcePart => SourcePart;

        ValueSyntax IStatementSyntax.ToValueSyntax(BinaryTree anchor)
            => CompoundSyntax.Create(T((IStatementSyntax)this), anchor);

        [DisableDump]
        ValueSyntax IStatementSyntax.Value => Value;

        IStatementSyntax IStatementSyntax.With(FrameItemContainer frameItems)
            => frameItems == null || !frameItems.Items.Any()
                ? this
                : Create(Declarer, Anchor, Value, frameItems.Combine(FrameItems));

        protected override Syntax GetDirectChildKernel(int index)
            => index switch
            {
                { } when index < LeftDirectChildCount => Declarer.GetDirectChild(index)
                , { } when index == LeftDirectChildCount => Value
                , _ => null
            };

        internal static IStatementSyntax Create
            (DeclarerSyntax declarer, BinaryTree anchor, ValueSyntax value, FrameItemContainer frameItems)
            => new DeclarationSyntax
            (
                declarer,
                anchor,
                value ?? new EmptyList(null, issue: IssueId.MissingDeclarationValue.Issue(anchor.Token.Characters)),
                frameItems
            );
    }
}