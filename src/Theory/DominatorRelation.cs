using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Theory
{
    sealed class DominatorRelation : DumpableObject
    {
        readonly string Characters;
        readonly string[] Data;

        readonly FunctionCache<string, int[]> Cache;

        public DominatorRelation(string characters, params string[] data)
        {
            Characters = characters;
            Data = data;
            Cache = new FunctionCache<string, int[]>(GetDominators);
        }

        bool IsLeftDominator(char left, char right)
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

        char DominatorChar(char left, char right)
        {
            var leftIndex = Characters.IndexWhere(x => x == left) ?? Characters.Length;
            var rightIndex = Characters.IndexWhere(x => x == right) ?? Characters.Length;
            return Data[leftIndex][rightIndex];
        }

        bool IsDominator(int position, string data)
        {
            var left = Dominators(data.LeftWord(position)).ToArray();
            if(left.Any(item => IsLeftDominator(data[item], data[position])))
                return false;

            var right = Dominators(data.RightWord(position)).ToArray();
            return right.All(item => IsLeftDominator(data[position], data[position + 1 + item]));
        }

        internal int[] Dominators(string data) => Cache[data];

        internal int[] GetDominators(string data)
            => data
                .Length
                .Select()
                .Where(position => IsDominator(position, data))
                .ToArray();
    }
}