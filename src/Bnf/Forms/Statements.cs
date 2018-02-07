using System.Linq;
using Bnf.CodeItems;
using Bnf.Contexts;
using Bnf.DataTypes;
using Bnf.Features;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Statements : Form, IStatements
    {
        [EnableDump]
        readonly IStatement[] Data;

        public Statements(Syntax parent, IStatement[] data)
            : base(parent) => Data = data;

        IStatement[] IListForm<IStatement>.Data => Data;

        protected override Result GetResult(Context context)
            => Result.Create
            (
                Parent.Token.Characters,
                DataType.Void,
                Data.Select(st => st.GetResult(context).ToVoid.CodeItems).Aggregate()
            );

    }
}