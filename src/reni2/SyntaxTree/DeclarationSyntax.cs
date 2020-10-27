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
            (DeclarerSyntax declarer, ValueSyntax value, Anchor frameItems = null)
            : base(anchor: frameItems ?? SyntaxTree.Anchor.Create())
        {
            Declarer = declarer;
            Value = value;

            Declarer.AssertIsNotNull();
            Value.AssertIsNotNull();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor.SourcePart.GetDumpAroundCurrent(5);

        [DisableDump]
        internal string NameOrNull => Declarer.Name?.Value;

        [DisableDump]
        internal bool IsConverterSyntax => Declarer.IsConverterSyntax;

        [DisableDump]
        internal bool IsMutableSyntax => Declarer.IsMutableSyntax;

        [DisableDump]
        protected override int LeftDirectChildCountInternal => Declarer.DirectChildCount;

        [DisableDump]
        protected override int DirectChildCount => LeftDirectChildCountInternal + 1;

        [DisableDump]
        DeclarerSyntax IStatementSyntax.Declarer => Declarer;

        SourcePart IStatementSyntax.SourcePart => Anchor.SourcePart;

        ValueSyntax IStatementSyntax.ToValueSyntax()
            => CompoundSyntax.Create(T((IStatementSyntax)this));

        [DisableDump]
        ValueSyntax IStatementSyntax.Value => Value;

        IStatementSyntax IStatementSyntax.With(Anchor frameItems)
            => frameItems == null || !frameItems.Items.Any()
                ? this
                : Create(Declarer, Value, frameItems.Combine(Anchor));

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                { } when index < LeftDirectChildCountInternal => Declarer.GetDirectChild(index)
                , { } when index == LeftDirectChildCountInternal => Value
                , _ => null
            };

        internal static IStatementSyntax Create
            (DeclarerSyntax declarer, ValueSyntax value, Anchor frameItems)
            => new DeclarationSyntax
            (
                declarer,
                value ?? new EmptyList(null, issue: IssueId.MissingDeclarationValue.Issue(frameItems.SourcePart)),
                frameItems
            );
    }
}