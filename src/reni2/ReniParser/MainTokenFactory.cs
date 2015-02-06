using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class MainTokenFactory : TokenFactory<TokenClass, Syntax>
    {
        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.Left
                    (
                        "_A_T_",
                        "_N_E_X_T_",
                        "to_number_of_base"
                    );

                x += PrioTable.Left(ArrayAccess.Id);
                x += PrioTable.Left(ConcatArrays.Id, ConcatArrays.MutableId);

                x += PrioTable.Left("~");
                x += PrioTable.Left("&");
                x += PrioTable.Left("|");

                x += PrioTable.Left("*", "/", "\\");
                x += PrioTable.Left("+", Minus.Id);

                x += PrioTable.Left("<", ">", "<=", ">=");
                x += PrioTable.Left("=", "<>");

                x += PrioTable.Left("!~");
                x += PrioTable.Left("!&!");
                x += PrioTable.Left("!|!");

                x += PrioTable.Right(ReassignToken.Id);

                x = x.ThenElseLevel("then", "else");
                x += PrioTable.Right("!");
                x += PrioTable.Left("/\\", "/!\\", "/\\/\\");
                x += PrioTable.Right(":");
                x += PrioTable.Right(",");
                x += PrioTable.Right(";");
                x += PrioTable.Right(".");
                x = x.ParenthesisLevelLeft
                    (
                        new[] {"(", "[", "{"},
                        new[] {")", "]", "}"}
                    );
                //x.Correct("(", PrioTable.Any, '-');
                //x.Correct("[", PrioTable.Any, '-');
                //x.Correct("{", PrioTable.Any, '-');

                x += PrioTable.Right(PrioTable.Error);

                x = x.ParenthesisLevelLeft(new[] {PrioTable.BeginOfText}, new[] {PrioTable.EndOfText});

                //Tracer.FlaggedLine("\n"+x.ToString());
                return x;
            }
        }


        public readonly IParser<Syntax> Parser;

        readonly ISubParser<Syntax> _declarationSyntaxSubParser;
        readonly PrioParser<Syntax> _declarationSyntaxParser;

        public MainTokenFactory()
        {
            Parser = new PrioParser<Syntax>(PrioTable, new Scanner<Syntax>(ReniLexer.Instance), this);
            _declarationSyntaxParser = new PrioParser<Syntax>
                (
                DeclarationTokenFactory.PrioTable,
                new Scanner<Syntax>(ReniLexer.Instance),
                new DeclarationTokenFactory()
                );
            _declarationSyntaxSubParser = new SubParser<Syntax>(_declarationSyntaxParser, Pack);
        }

        public bool Trace
        {
            get { return Parser.Trace; }
            set
            {
                Parser.Trace = value;
                _declarationSyntaxParser.Trace = value;
            }
        }


        /// <summary>
        ///     Creates the main token classes.
        /// </summary>
        /// <returns> </returns>
        protected override IDictionary<string, TokenClass> GetPredefinedTokenClasses()
        {
            return TokenClassesEx
                .ToDictionary(t => t.Id, t => (TokenClass) t)
                .Concat(TokenClasses)
                .ToDictionary(t => t.Key, t => t.Value);
        }

        IDictionary<string, TokenClass> TokenClasses => new Dictionary<string, TokenClass>
        {
            {"{", new LeftParenthesis(1)},
            {"[", new LeftParenthesis(2)},
            {"(", new LeftParenthesis(3)},
            {"}", new RightParenthesis(1)},
            {"]", new RightParenthesis(2)},
            {")", new RightParenthesis(3)},
            {"^^", new ContextOperator()},
            {".", new List()},
            {",", new List()},
            {";", new List()},
            {"@", new AtOperator()},
            {":", new Colon()},
            {"=", new CompareOperation()},
            {">", new CompareOperation()},
            {">=", new CompareOperation()},
            {"<", new CompareOperation()},
            {"<=", new CompareOperation()},
            {"<>", new CompareOperation()},
            {"!", new Exclamation(_declarationSyntaxSubParser)},
            {"+", new Plus()},
            {"/", new Slash()},
            {"/\\", new TokenClasses.Function()},
            {"/!\\", new TokenClasses.Function(true)},
            {"/\\/\\", new TokenClasses.Function(isMetaFunction: true)},
            {"*", new Star()},
            {"_A_T_", new AtToken()},
            {"dump_print", new DumpPrintToken()},
            {"else", new ElseToken()},
            {"enable_cut", new EnableCut()},
            {"function_instance", new FunctionInstanceToken()},
            {"instance", new InstanceToken()},
            {"new_value", new NewValueToken()},
            {"then", new ThenToken()},
            {"to_number_of_base", new ToNumberOfBase()},
            {"type", new TypeOperator()}
        };

        static IEnumerable<ITokenClassWithId> TokenClassesEx => new ITokenClassWithId[]
        {
            new ArrayAccess(),
            new ArrayReference(),
            new AlignToken(),
            new ArgToken(),
            new ConcatArrays(false),
            new ConcatArrays(true),
            new Count(),
            new EnableReinterpretation(),
            new ForceMutabilityToken(),
            new Minus(),
            new Mutable(),
            new Negate(),
            new ReassignToken(),
            new Reference(),
            new TextItem()
        };

        static IType<Syntax> Pack(Syntax options) { return new SyntaxBoxToken(options); }

        protected override TokenClass GetEndOfText() { return new EndToken(); }
        protected override TokenClass GetNumber() { return new Number(); }
        protected override TokenClass GetTokenClass(string name) { return new UserSymbol(name); }
        protected override TokenClass GetError(Match.IError message) { return new SyntaxError(message); }
        protected override TokenClass GetText() { return new Text(); }
    }


    sealed class SyntaxError : TokenClass
    {
        readonly IssueId _issue;
        public SyntaxError(IssueId issue) { _issue = issue; }
        public SyntaxError(Match.IError message) { _issue = ReniLexer.Parse(message); }
        protected override Syntax Terminal(SourcePart token) { return new CompileSyntaxError(_issue, token); }
        protected override Syntax Suffix(Syntax left, SourcePart token) { return left.SyntaxError(_issue, token); }
    }
}