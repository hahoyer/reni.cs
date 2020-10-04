using hw.DebugFormatter;

namespace Reni.Parser
{
    sealed class ReplaceArgVisitor : DumpableObject, ISyntaxVisitor
    {
        readonly ValueSyntax Syntax;
        public ReplaceArgVisitor(ValueSyntax syntax) { Syntax = syntax; }
        ValueSyntax ISyntaxVisitor.Arg => Syntax;
    }
}