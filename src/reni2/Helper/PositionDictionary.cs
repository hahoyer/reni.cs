using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Helper
{
    sealed class PositionDictionary<TResult> : DumpableObject
        where TResult : ITree<TResult>
    {
        readonly Dictionary<SourcePart, TResult> Value = new Dictionary<SourcePart, TResult>();

        internal TResult this[SourcePart key]
        {
            get => Value[key];
            set
            {
                (!Value.ContainsKey(key)).Assert(() => $@"
Key: {key.GetDumpAroundCurrent(5)} 

First: {Tracer.Dump(Value[key])}

This: {Tracer.Dump(value)}

First => This: {Value[key].GetPath(node=>ReferenceEquals(node, value)).Stringify(".")}
This => First: {value.GetPath(node=>ReferenceEquals(node, Value[key])).Stringify(".")}


");
                Value[key] = value;
            }
        }

        internal TResult this[BinaryTree key]
        {
            get => this[KeyMap(key)];
            set => this[KeyMap(key)] = value;
        }

        static SourcePart KeyMap(BinaryTree key) => key.Token.Characters;

        static int Position;

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
            var result = Value.ContainsKey(KeyMap(key));
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