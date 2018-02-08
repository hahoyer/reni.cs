using System.Linq;
using Bnf.Contexts;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;

namespace Bnf
{
    sealed class InterfaceContext : DumpableObject, IContext
    {
        string IContext.Define(string name, IExpression expression) => "interface " + name + "{}";
        string IContext.Statements(IStatement[] data) => data.Select(i => i.GetResult(this)).Stringify("\n\n");
    }
}