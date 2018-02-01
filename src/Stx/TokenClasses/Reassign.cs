using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Stx.CodeItems;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;
using Stx.Forms;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Reassign : TokenClass
    {
        public const string TokenId = ":=";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left.Form;
            var right = parent.Right.Form;
            return FormBase.CreateReassign(parent, left, right);
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class Colon : TokenClass
    {
        public const string TokenId = ":";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class Of : TokenClass
    {
        public const string TokenId = "OF";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }
}