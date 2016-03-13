using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using hw.Helper;

namespace ReniUI.RestFul
{
    public sealed class Channel
    {
        readonly ValueCache<CompilerBrowser> Compiler;
        string _text;

        public Channel() { Compiler = new ValueCache<CompilerBrowser>(CreateCompiler); }

        CompilerBrowser CreateCompiler() 
            => CompilerBrowser.FromText(_text);

        public string Text
        {
            get { return _text; }
            set
            {
                if(value == _text)
                    return;

                _text = value;
                Compiler.IsValid = false;
            }
        }

        public string GetResult() => Compiler.Value.FlatExecute();
    }
}