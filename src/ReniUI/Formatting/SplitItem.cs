using hw.DebugFormatter;
using hw.Helper;
using Reni.Parser;

namespace ReniUI.Formatting
{
    abstract class SplitItem : DumpableObject
    {
        internal static readonly FunctionCache<ITokenClass, SplitItem> List
            = new FunctionCache<ITokenClass, SplitItem>(tokenClass => new ListType {TokenClass = tokenClass});

        internal static readonly FunctionCache<ITokenClass, SplitItem> ColonLabel
            = new FunctionCache<ITokenClass, SplitItem>(tokenClass => new ColonLabelType {TokenClass = tokenClass});

        internal static readonly FunctionCache<ITokenClass, SplitItem> ColonBody
            = new FunctionCache<ITokenClass, SplitItem>(tokenClass => new ColonBodyType {TokenClass = tokenClass});

        internal static readonly FunctionCache<ITokenClass, SplitItem> LeftParenthesis
            = new FunctionCache<ITokenClass, SplitItem>(tokenClass => new LeftParenthesisType {TokenClass = tokenClass});

        internal static readonly FunctionCache<ITokenClass, SplitItem> RightParenthesis
            = new FunctionCache<ITokenClass, SplitItem>(tokenClass => new RightParenthesisType {TokenClass = tokenClass});

        class LeftParenthesisType : SplitItem {}

        class RightParenthesisType : SplitItem {}

        class ListType : SplitItem
        {
            internal override IndentDirection Indent => IndentDirection.NoIndent;
        }

        class ColonLabelType : SplitItem
        {
            internal override IndentDirection Indent => IndentDirection.NoIndent;

        }

        class ColonBodyType : SplitItem
        {
            internal override IndentDirection Indent => IndentDirection.ToRight;

        }

        internal ITokenClass TokenClass {get; private set;}

        internal virtual IndentDirection Indent
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }
    }



    class SplitMaster : DumpableObject
    {
        internal static readonly FunctionCache<ITokenClass, SplitMaster> List
            = new FunctionCache<ITokenClass, SplitMaster>(tokenClass => new ListType {TokenClass = tokenClass});

        internal static readonly FunctionCache<ITokenClass, SplitMaster> Colon
            = new FunctionCache<ITokenClass, SplitMaster>(tokenClass => new ColonType {TokenClass = tokenClass});

        internal static readonly FunctionCache<ITokenClass, SplitMaster> Parenthesis
            = new FunctionCache<ITokenClass, SplitMaster>(tokenClass => new ParenthesisType {TokenClass = tokenClass});

        class ParenthesisType : SplitMaster {}

        class ListType : SplitMaster {}

        class ColonType : SplitMaster {}

        internal ITokenClass TokenClass {get; private set;}
    }
}