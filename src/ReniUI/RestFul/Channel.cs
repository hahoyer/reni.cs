using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace ReniUI.RestFul
{
    public sealed class Channel
    {
        CompilerBrowser Compiler;
        string _text;

        public string Id;

        public string Text
        {
            get { return _text; }
            set
            {
                if(value == _text)
                    return;

                _text = value;

                Compiler = CompilerBrowser.FromText(_text, id: Id);
            }
        }

        [ScriptIgnore]
        public string Result => Compiler.FlatExecute(Id);
    }
}