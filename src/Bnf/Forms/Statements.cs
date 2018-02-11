using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Bnf.Forms
{
    sealed class Statements : Form, IStatements
    {
        [EnableDump]
        readonly IStatement[] Data;

        public Statements(Syntax parent, IStatement[] data)
            : base(parent) => Data = data;

        IStatement[] IListForm<IStatement>.Data => Data;
    }
}