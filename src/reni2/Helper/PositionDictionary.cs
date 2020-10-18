using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Helper
{
    sealed class PositionDictionary<TResult> : DumpableObject
    {
        readonly Dictionary<SourcePosition, TResult> Value = new Dictionary<SourcePosition, TResult>();

        internal TResult this[SourcePosition key]
        {
            get => Value[key];
            set
            {
                (!Value.ContainsKey(key)).Assert();
                Value[key] = value;
            }
        }

        internal TResult this[BinaryTree key]
        {
            get => this[KeyMap(key)];
            set => this[KeyMap(key)] = value;
        }

        static SourcePosition KeyMap(BinaryTree key) => key.Token.Characters.Start;

        public void AssertValid(BinaryTree binary)
            => binary.GetNodesFromLeftToRight().All(IsValidKey).Assert();

        bool IsValidKey(BinaryTree key)
        {
            var result = Value.ContainsKey(KeyMap(key));
            if(!result)
                key.Token.Characters.GetDumpAroundCurrent(5).Log();
            return result;
        }
    }
}