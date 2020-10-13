using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Helper
{
    sealed class Syntax : DumpableObject, ITree<Syntax>
    {
        internal readonly BinaryTree Binary;
        internal readonly Reni.Parser.Syntax FlatItem;
        internal readonly Syntax Parent;

        internal Syntax(Reni.Parser.Syntax flatItem, BinaryTree binary, Syntax parent)
        {
            FlatItem = flatItem;
            Binary = binary;
            Parent = parent;
        }

        [DisableDump]
        internal SourcePart SourcePart
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public IToken Token
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public ITokenClass TokenClass
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public Syntax Left
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public Syntax Right
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public Issue[] Issues
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public Syntax LeftMostRightSibling
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public IEnumerable<Syntax> ParentChainIncludingThis
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public string[] DeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        int ITree<Syntax>.DirectChildCount
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        Syntax ITree<Syntax>.GetDirectChild(int index)
        {
            NotImplementedMethod(index);
            return default;
        }

        [DisableDump]
        int ITree<Syntax>.LeftDirectChildCount => 0;

        public Syntax LocateByPosition(int offset)
        {
            NotImplementedMethod(offset);
            return default;
        }

        public Syntax Locate(SourcePart span)
        {
            NotImplementedMethod(span);
            return default;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this
                .GetNodesFromLeftToRight()
                .SelectMany(node => node?.ItemsAsLongAs(condition) ?? new Syntax[0]);
    }
}