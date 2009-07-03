using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;

namespace Reni.Parser
{
    /// <summary>
    /// Priority table used in parsing to create the syntax tree.
    /// </summary>
    [Serializable]
    internal sealed class PrioTable
    {
        private string[] _token;
        private char[,] _data;

        /// <summary>
        /// asis
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var x = obj as PrioTable;
            if(x == null)
                return false;
            return _token == x._token && _data == x._data;
        }

        /// <summary>
        /// asis
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return ReferenceEquals(y, null);
            return x.Equals(y);
        }

        /// <summary>
        /// asis
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() { return _token.GetHashCode() + _data.GetHashCode(); }

        /// <summary>
        /// asis
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return !ReferenceEquals(y, null);
            return !x.Equals(y);
        }

        /// <summary>
        /// shows the table in table form.
        /// The characters have the following meaning:
        /// Plus: New token is higher the found token, 
        /// Minus: Found token is higher than new token
        /// Equal sign: New token and found token are matching
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var maxlen = 0;
            for(var i = 0; i < Length; i++)
                if(maxlen < _token[i].Length)
                    maxlen = _token[i].Length;
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
                    result += _data[i, j];
                result += "\n";
            }
            return head0 + "\n" + head1 + "\n" + result;
        }

        private void AllocData(params string[][] x)
        {
            var l = 0;
            for(var i = 0; i < x.Length; i++)
                l += x[i].Length;
            _token = new string[l];
            var k = 0;
            for(var i = 0; i < x.Length; i++)
                for(var j = 0; j < x[i].Length; j++)
                {
                    _token[k] = x[i][j];
                    k++;
                }
            _data = new char[Length,Length];
            for(var i = 0; i < Length; i++)
                for(var j = 0; j < Length; j++)
                    _data[i, j] = ' ';
        }

        /// <summary>
        /// Returns number of token in table
        /// </summary>
        private int Length { get { return _token.Length; } }

        private PrioTable(char data, string[] token)
        {
            AllocData(token);
            for(var i = 0; i < Length; i++)
                for(var j = 0; j < Length; j++)
                    _data[i, j] = data;
        }

        private PrioTable(PrioTable x, PrioTable y)
        {
            AllocData(x._token, y._token);
            for(var i = 0; i < Length; i++)
                if(i < x.Length)
                    for(var j = 0; j < Length; j++)
                    {
                        _data[i, j] = '+';
                        if(j < x.Length)
                            _data[i, j] = x._data[i, j];
                    }
                else
                    for(var j = 0; j < Length; j++)
                    {
                        _data[i, j] = '-';
                        if(j >= x.Length)
                            _data[i, j] = y._data[i - x.Length, j - x.Length];
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

        private PrioTable(PrioTable x, string[] data, string[] left, string[] right)
        {
            AllocData(left, x._token, right);
            for(var i = 0; i < Length; i++)
                for(var j = 0; j < Length; j++)
                {
                    var iData = Find(i, left, x._token);
                    var jData = Find(j, left, x._token);
                    _data[i, j] = data[iData][jData];

                    if(iData == 1 && jData == 1)
                        _data[i, j] = x._data[i - left.Length, j - left.Length];
                    else if(iData == 2 && jData == 0)
                        if(j < i - left.Length - x.Length)
                            _data[i, j] = '-';
                        else if(j == i - left.Length - x.Length)
                            _data[i, j] = '=';
                        else
                            _data[i, j] = '+';
                }
        }

        /// <summary>
        /// Obtain the index in token list. 
        /// Empty string is considered as "end" in angle brackets. 
        /// If name is not found the entry "else" in angle brackets is used
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int Index(string name)
        {
            for(var i = 0; i < Length; i++)
                if(_token[i] == name)
                    return (i);

            for(var i = 0; i < Length; i++)
                if(_token[i] == "<else>")
                    return (i);
            throw new NotImplementedException("missing <else> entry in priority table");
        }

        /// <summary>
        /// Define a prio table with thokens that have the sam priority and are left associative
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static PrioTable LeftAssoc(params string[] x) { return new PrioTable('-', x); }

        /// <summary>
        /// Define a prio table with thokens that have the sam priority and are right associative
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static PrioTable RightAssoc(params string[] x) { return new PrioTable('+', x); }

        /// <summary>
        /// Define a prio table that adds a parenthesis level. 
        /// LToken and RToken shopuld have the same number of elements. 
        /// Elements of these lists that have the same index are consiedered as matching
        /// </summary>
        /// <param name="data">contains a 3 by 3 character table.
        /// The characters have the following meaning: 
        /// 0,0: left TokenClass finds left TokenClass;
        /// 0,1: left TokenClass finds TokenClass defined so far;
        /// 0,2: left TokenClass finds right TokenClass;
        /// 1,0: TokenClass defined so far finds left TokenClass;
        /// 1,1: ignored, Table for already defined Tokens used (use '?');
        /// 1,2: TokenClass defined so far finds right TokenClass;
        /// 2,0: ignored, "-=+"-Table generated (use '?');
        /// 2,1: right TokenClass finds TokenClass defined so far;
        /// 2,1: right TokenClass finds TokenClass defined so far;
        /// 2,2: right TokenClass finds right TokenClass
        /// </param>
        /// <param name="lToken">list of strings that play the role of left parenthesis</param>
        /// <param name="rToken">list of strings that play the role of right parenthesis</param>
        /// <returns></returns>
        public PrioTable ParLevel(string[] data, string[] lToken, string[] rToken) { return new PrioTable(this, data, lToken, rToken); }

        /// <summary>
        /// Combines two prioritity tables. The tokens contained in left operand are considered as higher priority operands
        /// </summary>
        /// <param name="x">higher priority tokens</param>
        /// <param name="y">lower priority tokens</param>
        /// <returns></returns>
        public static PrioTable operator +(PrioTable x, PrioTable y) { return new PrioTable(x, y); }

        /// <summary>
        /// Combines two prioritity tables. The tokens contained in left operand are considered as higher priority operands
        /// </summary>
        /// <param name="x">higher priority tokens</param>
        /// <param name="y">lower priority tokens</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static PrioTable Add(PrioTable x, PrioTable y) { return new PrioTable(x, y); }

        /// <summary>
        /// Manual correction of table entries
        /// </summary>
        /// <param name="n"></param>
        /// <param name="t"></param>
        /// <param name="d"></param>
        [UsedImplicitly]
        public void Correct(string n, string t, char d) { _data[Index(n), Index(t)] = d; }

        /// <summary>
        /// List of names, without the special tokens "frame", "end" and "else" in angle brackets
        /// </summary>
        [UsedImplicitly]
        public string[] GetNameList()
        {
            var result = new string[NormalNameLength()];
            var k = 0;
            for(var i = 0; i < Length; i++)
                if(IsNormalName(_token[i]))
                    result[k++] = _token[i];
            return result;
        }

        private int NormalNameLength()
        {
            var n = 0;
            for(var i = 0; i < Length; i++)
                if(IsNormalName(_token[i]))
                    n++;
            return n;
        }

        private static bool IsNormalName(string name) { return name != "<frame>" && name != "<end>" && name != "<else>"; }

        /// <summary>
        /// Returns the priority information of a pair of tokens
        /// The characters have the following meaning:
        /// Plus: New token is higher the found token, 
        /// Minus: Found token is higher than new token
        /// Equal sign: New token and found token are matching
        /// </summary>
        /// <param name="newToken"></param>
        /// <param name="recentToken"></param>
        /// <returns></returns>
        public char Relation(Token newToken, Token recentToken)
        {
            //Tracer.FlaggedLine("\"" + _token[New] + "\" on \"" + _token[recentToken] + "\" --> \"" + _data[New, recentToken] + "\"");
            return _data[newToken.Index(this), recentToken.Index(this)];
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
                var result = new string[_data.GetLength(0)];
                for(var i = 0; i < _data.GetLength(0); i++)
                {
                    result[i] = "";
                    for(var j = 0; j < _data.GetLength(1); j++)
                        result[i] += _data[i, j];
                }
                return result;
            }
        }
    }
}