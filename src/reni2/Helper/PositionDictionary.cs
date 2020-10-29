using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Helper
{
    public sealed class PositionDictionary<TResult> : DumpableObject
        where TResult : ITree<TResult>
    {
        static int Position;

        readonly IDictionary<SourcePosition, (int Length, TResult Node)[]> Value =
            new Dictionary<SourcePosition, (int, TResult)[]>();

        internal(int Length, TResult Node)[] this[SourcePosition key] => Value[key];

        internal TResult this[SourcePart keyPart]
        {
            get => Value[keyPart.Start].Single(node => node.Length == keyPart.Length).Node;
            set
            {
                for(var offset = 0; offset <= keyPart.Length; offset++)
                {
                    var key = keyPart.Start + offset;
                    if(!Value.TryGetValue(key, out var currentValue))
                        currentValue = new (int, TResult)[0];

                    Value[key] = currentValue.Concat(T((keyPart.Length - offset, value))).ToArray();
                }
            }
        }

        internal TResult this[BinaryTree key]
        {
            get => this[KeyMap(key)];
            set => this[KeyMap(key)] = value;
        }

        static SourcePart KeyMap(BinaryTree key) => key.Token.SourcePart();

        public void AssertValid(BinaryTree binary)
        {
            lock(this)
            {
                Position = 0;
                binary
                    .GetNodesFromLeftToRight()
                    .Select(IsValidKey)
                    .ToArray()
                    .All(i => i)
                    .Assert();
            }
        }

        bool IsValidKey(BinaryTree key)
        {
            var result = Value.ContainsKey(KeyMap(key).Start);
            if(result)
                return true;
            @$" 
 -----
/{Position++:D5}\    BinaryTree.ObjectId={key.ObjectId}
Key: -----------------------------------------vv
{key.Token.Characters.GetDumpAroundCurrent(25)}
----------------------------------------------^^".Log();

            return false;
        }
    }
}