#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Parser;
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class MainTokenFactory : Parser.TokenFactory<TokenClasses.TokenClass>
    {
        protected override PrioTable GetPrioTable()
        {
            var x = PrioTable.Left(PrioTable.Common);
            x += PrioTable.Left
                ("reference"
                 , "_A_T_", "_N_E_X_T_"
                 , "to_number_of_base"
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

            x = x.Level
                (new[]
                {
                    "+--",
                    "+?+",
                    "?-+"
                },
                 new[] {"then"},
                 new[] {"else"}
                );
            x += PrioTable.Right("!");
            x += PrioTable.Left("/\\", "/!\\", "/\\/\\", "/!\\/!\\");
            x += PrioTable.Right(":");
            x += PrioTable.Right(",");
            x += PrioTable.Right(";");
            x = x.Level
                (new[]
                {
                    "++-",
                    "+?-",
                    "?--"
                },
                 new[] {"(", "[", "{"},
                 new[] {")", "]", "}"}
                );
            //x.Correct("(", PrioTable.Common, '-');
            //x.Correct("[", PrioTable.Common, '-');
            //x.Correct("{", PrioTable.Common, '-');

            x += PrioTable.Right(PrioTable.SyntaxError);

            x = x.Level
                (new[]
                {
                    "++-",
                    "+?-",
                    "?--"
                },
                 new[] { PrioTable.Frame },
                 new[] { PrioTable.End }
                );

            //Tracer.FlaggedLine("\n"+x.ToString());
            return x;
        }

        /// <summary>
        ///     Creates the main token classes.
        /// </summary>
        /// <returns> </returns>
        protected override DictionaryEx<string, TokenClasses.TokenClass> GetTokenClasses() { return TokenClasses; }

        internal new static DictionaryEx<string, TokenClasses.TokenClass> TokenClasses
        {
            get
            {
                return new DictionaryEx<string, TokenClasses.TokenClass>
                {
                    {"@", new AtOperator()},
                    {"^", new ContextOperator()},
                    {":", new Colon()},
                    {":=", new Assignment()},
                    {"=", new Equal()},
                    {">", new CompareOperator()},
                    {">=", new CompareOperator()},
                    {"<", new CompareOperator()},
                    {"<=", new CompareOperator()},
                    {"<>", new NotEqual()},
                    {"<<", new ConcatArrays()},
                    {"-", new Sign()},
                    {"!", new Exclamation()},
                    {"+", new Sign()},
                    {"/", new Slash()},
                    {"/\\", new TokenClasses.Function()},
                    {"/!\\", new TokenClasses.Function(isImplicit: true)},
                    {"/\\/\\", new TokenClasses.Function(isMetaFunction: true)},
                    {"/!\\/!\\", new TokenClasses.Function(isImplicit: true, isMetaFunction: true)},
                    {"*", new Star()},
                    {"_A_T_", new AtToken()},
                    {"arg", new ArgToken()},
                    {"dump_print", new DumpPrintToken()},
                    {"else", new ElseToken()},
                    {"enable_cut", new EnableCut()},
                    {"enable_raw_conversion", new EnableRawConversion()},
                    {"function_instance", new FunctionInstanceToken()},
                    {"instance", new InstanceToken()},
                    {"new_value", new NewValueToken()},
                    {"reference", new ReferenceToken()},
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

        protected override TokenClasses.TokenClass GetListClass() { return new List(); }
        protected override TokenClasses.TokenClass GetRightParenthesisClass(int level) { return new RightParenthesis(level); }
        protected override TokenClasses.TokenClass GetLeftParenthesisClass(int level) { return new LeftParenthesis(level); }
        protected override TokenClasses.TokenClass GetNumberClass() { return new TokenClasses.Number(); }
        protected override TokenClasses.TokenClass GetNewTokenClass(string name) { return new UserSymbol(name); }
        protected override TokenClasses.TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
        protected override TokenClasses.TokenClass GetTextClass() { return new Text(); }
    }

    sealed class SyntaxError : TokenClasses.TokenClass
    {
        readonly string _message;
        public SyntaxError(string message) { _message = message; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}