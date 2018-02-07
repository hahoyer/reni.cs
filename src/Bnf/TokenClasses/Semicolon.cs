using System.Linq;
using Bnf.Forms;
using Bnf.Scanner;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Semicolon : TokenClass
    {
        public const string TokenId = ";";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left =  parent.Left?.Form.Checked<IStatement>(parent);
            var rightForm = parent.Right?.Form;
            var right = rightForm.MakeStatements();
            if(right != null)
            {
                if(left is IStatement statement)
                    return new Statements(parent, new[] {statement}.Concat(right).ToArray());
                return left;
            }

            if(rightForm is IList rightList)
                return new List(parent, left, rightList.Data);
            
            NotImplementedMethod(parent);
            return null;
        }
    }
}