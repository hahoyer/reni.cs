using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;
using Reni;
using Reni.UserInterface;
using ScintillaNET;

namespace ReniTest.CompilationView
{
    sealed class TextStyle : EnumEx
    {
        static int _nextId;
        [UsedImplicitly]
        public static readonly TextStyle Default = new TextStyle(Color.Black);
        [UsedImplicitly]
        public static readonly TextStyle KeyWord = new TextStyle(Color.Blue);
        [UsedImplicitly]
        public static readonly TextStyle BraceLikeKeyWord = new TextStyle(Color.Blue, true);
        [UsedImplicitly]
        public static readonly TextStyle Brace = new TextStyle(Color.Black, true);
        [UsedImplicitly]
        public static readonly TextStyle Comment = new TextStyle(Color.Green);
        [UsedImplicitly]
        public static readonly TextStyle Number = new TextStyle(Color.Purple);
        [UsedImplicitly]
        public static readonly TextStyle Text = new TextStyle(Color.Red);
        [UsedImplicitly]
        public static readonly TextStyle Error = new TextStyle(Color.Gray, isItalic: true);

        static public TextStyle From(Token token, Compiler compiler)
        {
            if(token.IsError)
                return Error;
            if(token.IsBraceLike && token.FindAllBelongings(compiler).Skip(1).Any())
                return token.IsBrace ? Brace : BraceLikeKeyWord;
            if(token.IsKeyword)
                return KeyWord;
            if(token.IsComment || token.IsLineComment)
                return Comment;
            if(token.IsNumber)
                return Number;
            if(token.IsText)
                return Text;
            return Default;
        }

        public static implicit operator int(TextStyle v) => v.Id;
        public static IEnumerable<TextStyle> All => AllInstances<TextStyle>();

        readonly int Id;
        readonly Color ForeColor;
        readonly bool IsItalic;
        readonly bool IsBold;

        TextStyle(Color foreColor, bool isBold = false, bool isItalic = false)
        {
            ForeColor = foreColor;
            IsBold = isBold;
            IsItalic = isItalic;
            Id = _nextId++;
        }

        internal void Config(Style style)
        {
            style.Font = "Lucida Console";
            style.Size = 10;
            style.ForeColor = ForeColor;
            style.Italic = IsItalic;
            style.Bold = IsBold;
        }
    }
}