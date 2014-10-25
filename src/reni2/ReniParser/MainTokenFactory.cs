using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;

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

                x += PrioTable.Left("<<");

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

                x += PrioTable.Right(":=");

                x = x.ThenElseLevel("then", "else");
                x += PrioTable.Right("!");
                x += PrioTable.Left("/\\", "/!\\", "/\\/\\");
                x += PrioTable.Right(":");
                x += PrioTable.Right(",");
                x += PrioTable.Right(";");
                x += PrioTable.Right(".");
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
        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses() { return TokenClasses; }

        internal static FunctionCache<string, TokenClass> TokenClasses
        {
            get
            {
                return new FunctionCache<string, TokenClass>
                {
                    {AlignToken.Id, new AlignToken()},
                    {ArgToken.Id, new ArgToken()},
                    {Minus.Id, new Minus()},
                    {Negate.Id, new Negate()},
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
                    {":=", new Assignment()},
                    {"=", new CompareOperation()},
                    {">", new CompareOperation()},
                    {">=", new CompareOperation()},
                    {"<", new CompareOperation()},
                    {"<=", new CompareOperation()},
                    {"<>", new CompareOperation()},
                    {"<<", new ConcatArrays()},
                    {"<:=>", new EnableReassignToken()},
                    {"!", new Exclamation()},
                    {"+", new Plus()},
                    {"/", new Slash()},
                    {"/\\", new TokenClasses.Function()},
                    {"/!\\", new TokenClasses.Function(true)},
                    {"/\\/\\", new TokenClasses.Function(isMetaFunction: true)},
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
                    {"text_item", new TextItem()},
                    {"then", new ThenToken()},
                    {"to_number_of_base", new ToNumberOfBase()},
                    {"undecorate", new UndecorateToken()},
                    {"type", new TypeOperator()}
                };
            }
        }

        protected override TokenClass GetEndOfText() { return new RightParenthesis(0); }
        protected override TokenClass GetNumber() { return new Number(); }
        protected override TokenClass GetTokenClass(string name) { return new UserSymbol(name); }
        protected override TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
        protected override TokenClass GetText() { return new Text(); }
    }

    sealed class SyntaxError : TokenClass
    {
        readonly string _message;
        public SyntaxError(string message) { _message = message; }
    }
}