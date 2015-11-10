using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;

namespace Theory
{
    sealed class DominatorRelation : DumpableObject
    {
        readonly string[] Characters;
        readonly string[] Data;

        readonly FunctionCache<string, string[]> Cache;

        public DominatorRelation(string characters0, string characters1, params string[] data)
        {
            Characters = characters0
                .Select((item, index) => ("" + item + characters1[index]).Trim())
                .ToArray();
            Data = data;
            Cache = new FunctionCache<string, string[]>(GetDominators);
        }

        bool IsLeftDominator(string left, string right)
        {
            var c = DominatorChar(left, right);
            switch(c)
            {
                case '+':
                    return true;
                case '-':
                    return false;
                default:
                    NotImplementedMethod(left, right, nameof(c), c);
                    return false;
            }
        }

        bool IsLeftDominator(char left, string right) => IsLeftDominator("" + left, right);
        bool IsLeftDominator(string left, char right) => IsLeftDominator(left, "" + right);

        char DominatorChar(string left, string right)
        {
            var leftIndex = Characters.IndexWhere(item => item == left).AssertValue();
            var rightIndex = Characters.IndexWhere(item => item == right).AssertValue();
            return Data[leftIndex][rightIndex];
        }

        Tuple<bool,bool> IsDominator(int position, string data)
        {
            var left = Dominators(data.LeftWord(position)).ToArray();
            if(left.Any(item => IsLeftDominator(item, data[position])))
                return false;

            var right = Dominators(data.RightWord(position)).ToArray();
            return right.All(item => IsLeftDominator(data[position], item));
        }

        internal string[] Dominators(string data) => Cache[data];

        internal string[] GetDominators(string data)
            => data
                .Length
                .Select()
                .Select
                (
                    position => new
                    {
                        position,
                        isDominator = IsDominator(position, data),
                        isMatch= IsMatch(position, data)
                    }
                )
                .Where(item => IsDominator(position, data))
                .Select(item => (item.isDominator == true ? "" : "=") + data[item.position])
                .ToArray();
    }
}