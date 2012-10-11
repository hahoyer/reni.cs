using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;

namespace Reni.Parser
{
    /// <summary>
    ///     Priority table used in parsing to create the syntax tree.
    /// </summary>
    [Serializable]
    internal sealed class PrioTable
    {
        internal const string Common = "<common>";
        internal const string End = "<end>";
        internal const string Frame = "<frame>";
        internal const string SyntaxError = "<syntaxerror>";

        private readonly string[] _token;
        private readonly SimpleCache<char[,]> _dataCache;

        /// <summary>
        ///     asis
        /// </summary>
        /// <param name = "obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var x = obj as PrioTable;
            if(x == null)
                return false;
            return _token == x._token && _dataCache == x._dataCache;
        }

        public static bool operator ==(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return ReferenceEquals(y, null);
            return x.Equals(y);
        }

        /// <summary>
        ///     asis
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() { return _token.GetHashCode() + _dataCache.GetHashCode(); }

        public static bool operator !=(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return !ReferenceEquals(y, null);
            return !x.Equals(y);
        }

        /// <summary>
        ///     shows the table in table form.
        ///     The characters have the following meaning:
        ///     Plus: New token is higher the found token, 
        ///     Minus: Found token is higher than new token
        ///     Equal sign: New token and found token are matching
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var maxlen = 0;
            for(var i = 0; i < Length; i++)
            {
                if(maxlen < _token[i].Length)
                    maxlen = _token[i].Length;
            }
            var head0 = "";
            head0 = head0.PadLeft(maxlen);
            head0 += "    ";
            var head1 = head0;
            var result = "";
            for(var i = 0; i < _token.Length; i++)
            {
                var ii = Convert.ToString(i + 10000);
                head0 += ii[3];
                head1 += ii[4];
                result += _token[i].PadLeft(maxlen) + " " + ii.Substring(3, 2) + " ";
                for(var j = 0; j < Length; j++)
                    result += _dataCache.Value[i, j];
                result += "\n";
            }
            return head0 + "\n" + head1 + "\n" + result;
        }

        char[,] AllocData()
        {
            var data = new char[Length,Length];
            for(var i = 0; i < Length; i++)
                for(var j = 0; j < Length; j++)
                    data[i, j] = ' ';
            return data;
        }
        static string[] AllocTokens(params string[][] tokenArrayList)
        {
            var l = tokenArrayList.Sum(t => t.Length);
            var tokens = new string[l];
            var k = 0;
            foreach(var tokenArray in tokenArrayList)
                foreach(var token in tokenArray)
                    tokens[k++] = token;
            return tokens;
        }

        /// <summary>
        ///     Returns number of token in table
        /// </summary>
        private int Length { get { return _token.Length; } }

        private PrioTable()
        {
            _dataCache = new SimpleCache<char[,]>(AllocData);
        }

        private PrioTable(char data, string[] token) : this()
        {
            _token = AllocTokens(token);
            for(var i = 0; i < Length; i++)
            {
                for(var j = 0; j < Length; j++)
                    _dataCache.Value[i, j] = data;
            }
        }

        private PrioTable(PrioTable x, PrioTable y): this()
        {
            _token = AllocTokens(x._token, y._token);
            for(var i = 0; i < Length; i++)
            {
                if(i < x.Length)
                {
                    for(var j = 0; j < Length; j++)
                    {
                        _dataCache.Value[i, j] = '+';
                        if(j < x.Length)
                            _dataCache.Value[i, j] = x._dataCache.Value[i, j];
                    }
                }
                else
                {
                    for(var j = 0; j < Length; j++)
                    {
                        _dataCache.Value[i, j] = '-';
                        if(j >= x.Length)
                            _dataCache.Value[i, j] = y._dataCache.Value[i - x.Length, j - x.Length];
                    }
                }
            }
        }

        private static int Find(int i, params string[][] x)
        {
            for(var j = 0; j < x.Length; j++)
            {
                i -= x[j].Length;
                if(i < 0)
                    return j;
            }
            return x.Length;
        }

        private PrioTable(PrioTable x, string[] data, string[] left, string[] right): this()
        {
            _token = AllocTokens(left, x._token, right);
            for(var i = 0; i < Length; i++)
            {
                for(var j = 0; j < Length; j++)
                {
                    var iData = Find(i, left, x._token);
                    var jData = Find(j, left, x._token);
                    _dataCache.Value[i, j] = data[iData][jData];

                    if(iData == 1 && jData == 1)
                        _dataCache.Value[i, j] = x._dataCache.Value[i - left.Length, j - left.Length];
                    else if(iData == 2 && jData == 0)
                    {
                        if(j < i - left.Length - x.Length)
                            _dataCache.Value[i, j] = '-';
                        else if(j == i - left.Length - x.Length)
                            _dataCache.Value[i, j] = '=';
                        else
                            _dataCache.Value[i, j] = '+';
                    }
                }
            }
        }

        /// <summary>
        ///     Obtain the index in token list. 
        ///     Empty string is considered as "end" in angle brackets. 
        ///     If name is not found the entry "common" in angle brackets is used
        /// </summary>
        /// <param name = "name"></param>
        /// <returns></returns>
        public int Index(string name)
        {
            for(var i = 0; i < Length; i++)
                if(_token[i] == name)
                    return (i);

            for(var i = 0; i < Length; i++)
                if(_token[i] == Common)
                    return (i);

            throw new NotImplementedException("missing "+Common+" entry in priority table");
        }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are left associative
        /// </summary>
        /// <param name = "x"></param>
        /// <returns></returns>
        public static PrioTable Left(params string[] x) { return new PrioTable('-', x); }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are right associative
        /// </summary>
        /// <param name = "x"></param>
        /// <returns></returns>
        public static PrioTable Right(params string[] x) { return new PrioTable('+', x); }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are like a list
        /// </summary>
        /// <param name = "x"></param>
        /// <returns></returns>
        public static PrioTable List(params string[] x) { return new PrioTable('=', x); }

        /// <summary>
        ///     Define a prio table that adds a parenthesis level. 
        ///     LToken and RToken should have the same number of elements. 
        ///     Elements of these lists that have the same index are consiedered as matching
        /// </summary>
        /// <param name = "data">contains a 3 by 3 character table.
        ///     The characters have the following meaning: 
        ///     0,0: left TokenClass finds left TokenClass;
        ///     0,1: left TokenClass finds TokenClass defined so far;
        ///     0,2: left TokenClass finds right TokenClass;
        ///     1,0: TokenClass defined so far finds left TokenClass;
        ///     1,1: ignored, Table for already defined Tokens used (use '?');
        ///     1,2: TokenClass defined so far finds right TokenClass;
        ///     2,0: ignored, "-=+"-Table generated (use '?');
        ///     2,1: right TokenClass finds TokenClass defined so far;
        ///     2,2: right TokenClass finds right TokenClass
        /// </param>
        /// <param name = "lToken">list of strings that play the role of left parenthesis</param>
        /// <param name = "rToken">list of strings that play the role of right parenthesis</param>
        /// <returns></returns>
        public PrioTable Level(string[] data, string[] lToken, string[] rToken) { return new PrioTable(this, data, lToken, rToken); }

        public static PrioTable operator +(PrioTable x, PrioTable y) { return new PrioTable(x, y); }

        /// <summary>
        ///     Combines two prioritity tables. The tokens contained in left operand are considered as higher priority operands
        /// </summary>
        /// <param name = "x">higher priority tokens</param>
        /// <param name = "y">lower priority tokens</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static PrioTable Add(PrioTable x, PrioTable y) { return new PrioTable(x, y); }

        /// <summary>
        ///     Manual correction of table entries
        /// </summary>
        /// <param name = "n"></param>
        /// <param name = "t"></param>
        /// <param name = "d"></param>
        [UsedImplicitly]
        public void Correct(string n, string t, char d) { _dataCache.Value[Index(n), Index(t)] = d; }

        /// <summary>
        ///     List of names, without the special tokens "frame", "end" and "else" in angle brackets
        /// </summary>
        [UsedImplicitly]
        public string[] GetNameList()
        {
            var result = new string[NormalNameLength()];
            var k = 0;
            for(var i = 0; i < Length; i++)
            {
                if(IsNormalName(_token[i]))
                    result[k++] = _token[i];
            }
            return result;
        }

        private int NormalNameLength()
        {
            var n = 0;
            for(var i = 0; i < Length; i++)
            {
                if(IsNormalName(_token[i]))
                    n++;
            }
            return n;
        }

        private static bool IsNormalName(string name) { return name != Frame && name != End && name != Common && name != SyntaxError; }

        /// <summary>
        ///     Returns the priority information of a pair of tokens
        ///     The characters have the following meaning:
        ///     Plus: New token is higher the recent token, 
        ///     Minus: Recent token is higher than new token
        ///     Equal sign: New token and recent token are matching
        /// </summary>
        /// <param name = "newTokenName"></param>
        /// <param name = "recentTokenName"></param>
        /// <returns></returns>
        public char Relation(string newTokenName, string recentTokenName)
        {
            //Tracer.FlaggedLine("\"" + _token[New] + "\" on \"" + _token[recentToken] + "\" --> \"" + _dataCache[New, recentToken] + "\"");
            return _dataCache.Value[Index(newTokenName), Index(recentTokenName)];
        }

        //For debug only
        [Node]
        public string[] Token { get { return _token; } }

        //For debug only
        [Node]
        public string[] Data
        {
            get
            {
                var result = new string[_dataCache.Value.GetLength(0)];
                for (var i = 0; i < _dataCache.Value.GetLength(0); i++)
                {
                    result[i] = "";
                    for (var j = 0; j < _dataCache.Value.GetLength(1); j++)
                        result[i] += _dataCache.Value[i, j];
                }
                return result;
            }
        }
    }
}