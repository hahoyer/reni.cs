using hw.DebugFormatter;

namespace Reni.Parser
{
    sealed class ReplaceArgVisitor : DumpableObject, ISyntaxVisitor
    {
        readonly Value _value;
        public ReplaceArgVisitor(Value value) { _value = value; }
        Value ISyntaxVisitor.Arg => _value;
    }
}