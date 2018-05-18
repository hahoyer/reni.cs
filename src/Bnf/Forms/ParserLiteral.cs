using hw.DebugFormatter;
using hw.Helper;

namespace Bnf.Forms
{
    sealed class ParserLiteral : DumpableObject, ILiteral
    {
        static string Parse(string name)
        {
            var result = "";
            for(var i = 0; i < name.Length; i++)
                if(name[i] == '\\')
                {
                    i++;
                    switch(name[i])
                    {
                        case 'n':
                            result += "\n";
                            break;
                        case 'r':
                            result += "\r";
                            break;
                        case 't':
                            result += "\t";
                            break;
                        case '\\':
                            result += "\\";
                            break;
                        default:
                            Tracer.Assert(false);
                            break;
                    }
                }
                else
                    result += name[i];

            return result;
        }

        readonly string Text;
        public ParserLiteral(string name) => Text = Parse(name);
        string IUniqueIdProvider.Value => Text;
    }
}