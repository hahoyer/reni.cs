using hw.DebugFormatter;

namespace Reni.Parser
{
    sealed class ReplaceArgVisitor : DumpableObject, ISyntaxVisitor
    {
        readonly Syntax Syntax;
        public ReplaceArgVisitor(Syntax syntax) { Syntax = syntax; }
        Syntax ISyntaxVisitor.Arg => Syntax;
    }
}