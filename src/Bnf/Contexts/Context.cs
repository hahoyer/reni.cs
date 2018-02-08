using Bnf.Forms;

namespace Bnf.Contexts
{
    interface IContext
    {
        string Define(string name, IExpression expression);
        string Statements(IStatement[] data);
    }
}