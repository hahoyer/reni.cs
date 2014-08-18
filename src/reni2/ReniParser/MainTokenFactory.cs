using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using hw.PrioParser;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class MainTokenFactory : hw.Parser.TokenFactory<TokenClasses.TokenClass>
    {
        public MainTokenFactory(PrioTable prioTable)
            : base(prioTable) { }

        public MainTokenFactory()
            : base(PrioTable) { }

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

                x += PrioTable.Left("<<");

                x += PrioTable.Left("~");
                x += PrioTable.Left("&");
                x += PrioTable.Left("|");

                x += PrioTable.Left("*", "/", "\\");
                x += PrioTable.Left("+", "-");

                x += PrioTable.Left("<", ">", "<=", ">=");
                x += PrioTable.Left("=", "<>");

                x += PrioTable.Left("!~");
                x += PrioTable.Left("!&!");
                x += PrioTable.Left("!|!");

                x += PrioTable.Right(":=", "prototype", ":+", ":-", ":*", ":/", ":\\");

                x = x.ThenElseLevel("then", "else");
                x += PrioTable.Right("!");
                x += PrioTable.Left("/\\", "/!\\", "/\\/\\", "/!\\/!\\");
                x += PrioTable.Right(":");
                x += PrioTable.Right(",");
                x += PrioTable.Right(";");
                x = x.ParenthesisLevel
                    (
                        new[] {"(", "[", "{"},
                        new[] {")", "]", "}"}
                    );
                //x.Correct("(", PrioTable.Any, '-');
                //x.Correct("[", PrioTable.Any, '-');
                //x.Correct("{", PrioTable.Any, '-');

                x += PrioTable.Right(PrioTable.Error);

                x = x.ParenthesisLevel(PrioTable.BeginOfText, PrioTable.EndOfText);

                //Tracer.FlaggedLine("\n"+x.ToString());
                return x;
            }
        }

        /// <summary>
        ///     Creates the main token classes.
        /// </summary>
        /// <returns> </returns>
        protected override FunctionCache<string, TokenClasses.TokenClass> GetPredefinedTokenClasses() { return TokenClasses; }

        internal static FunctionCache<string, TokenClasses.TokenClass> TokenClasses
        {
            get
            {
                return new FunctionCache<string, TokenClasses.TokenClass>
                {
                    {"{", new LeftParenthesis(1)},
                    {"[", new LeftParenthesis(2)},
                    {"(", new LeftParenthesis(3)},
                    {"}", new RightParenthesis(1)},
                    {"]", new RightParenthesis(2)},
                    {")", new RightParenthesis(3)},
                    {".", new ArgToken()},
                    {",", new List()},
                    {";", new List()},
                    {"@", new AtOperator()},
                    {"^", new ContextOperator()},
                    {":", new Colon()},
                    {":=", new Assignment()},
                    {"=", new CompareOperation()},
                    {">", new CompareOperation()},
                    {">=", new CompareOperation()},
                    {"<", new CompareOperation()},
                    {"<=", new CompareOperation()},
                    {"<>", new CompareOperation()},
                    {"<<", new ConcatArrays()},
                    {"-", new Sign()},
                    {"!", new Exclamation()},
                    {"+", new Sign()},
                    {"/", new Slash()},
                    {"/\\", new TokenClasses.Function()},
                    {"/!\\", new TokenClasses.Function(true)},
                    {"/\\/\\", new TokenClasses.Function(isMetaFunction: true)},
                    {"/!\\/!\\", new TokenClasses.Function(true, true)},
                    {"*", new Star()},
                    {"_A_T_", new AtToken()},
                    {"dump_print", new DumpPrintToken()},
                    {"else", new ElseToken()},
                    {"enable_array_oversize", new EnableArrayOverSize()},
                    {"enable_cut", new EnableCut()},
                    {"function_instance", new FunctionInstanceToken()},
                    {"instance", new InstanceToken()},
                    {"instance_from_raw_address", new InstanceFromRawAddress()},
                    {"new_value", new NewValueToken()},
                    {"raw_address", new RawAddress()},
                    {"sequence", new SequenceToken()},
                    {"text_item", new TextItem()},
                    {"text_items", new TextItems()},
                    {"then", new ThenToken()},
                    {"to_number_of_base", new ToNumberOfBase()},
                    {"undecorate", new UndecorateToken()},
                    {"type", new TypeOperator()}
                };
            }
        }

        protected override TokenClasses.TokenClass GetEndOfText() { return new RightParenthesis(0); }
        protected override TokenClasses.TokenClass GetBeginOfText() { return new LeftParenthesis(0); }
        protected override TokenClasses.TokenClass GetNumber() { return new Number(); }
        protected override TokenClasses.TokenClass GetTokenClass(string name) { return new UserSymbol(name); }
        protected override TokenClasses.TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
        protected override TokenClasses.TokenClass GetText() { return new Text(); }
    }

    sealed class SyntaxError : TokenClasses.TokenClass
    {
        readonly string _message;
        public SyntaxError(string message) { _message = message; }
    }
}