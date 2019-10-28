using Bnf.Forms;
using hw.Scanner;

namespace Bnf.Base
{
    interface IContext<T>
        where T : class, IParseSpan, ISourcePartProxy
    {
        OccurenceDictionary<T> CreateOccurence(Literal literal);
        OccurenceDictionary<T> CreateOccurence(Define.IDestination destination);
        OccurenceDictionary<T> CreateRepeat(OccurenceDictionary<T> children);
        OccurenceDictionary<T> CreateSequence(IExpression[] expressions);
    }
}