using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Stx.Forms;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Semicolon : TokenClass
    {
        public const string TokenId = ";";

        static (ISequenceItem[] items, IForm error) Checked(Syntax parent, IForm form)
        {
            if(form == null)
                return (new ISequenceItem[0], null);

            var result = (form as ISequence)?.Items;
            if(result != null)
                return (result, null);

            var @checked = Extension.Checked<ISequenceItem>(form, parent);
            var leftItem = @checked as ISequenceItem;
            return leftItem == null ? (null, @checked ) : (new[] {leftItem}, @checked );
        }

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left?.Form;
            var right = parent.Right?.Form;

            var leftResult = Checked(parent, left);
            var rightResult = Checked(parent, right);

            return leftResult.error ??
                   rightResult.error ?? 
                   new Sequence(parent, leftResult.items.Concat(rightResult.items));
        }
    }

}