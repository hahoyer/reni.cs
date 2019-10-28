using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using Reni;

namespace ReniUI.RestFul
{
    public sealed class Channel
    {
        readonly ValueCache<CompilerBrowser> CompilerCache;
        readonly ValueCache<StringStream> ResultCache;
        string _text;

        public Channel()
        {
            ResultCache = new ValueCache<StringStream>(GetResult);
            CompilerCache = new ValueCache<CompilerBrowser>(CreateCompiler);
        }

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
                CompilerCache.IsValid = false;
                ResultCache.IsValid = false;
            }
        }

        StringStream GetResult() => CompilerCache.Value.Result;

        public Issue[] GetIssues() => CompilerCache.Value.Issues.Select(Issue.Create).ToArray();

        public void ResetResult() { ResultCache.IsValid = false; }

        public string GetOutput()
        {
            if(GetIssues().Any())
                throw new ExecutionProhibitedException();
            return ResultCache.Value.Data;
        }

        sealed class ExecutionProhibitedException : Exception {}

        public string GetUnexpectedErrors()
        {
            if(GetIssues().Any())
                return "";
            return ResultCache.Value.Log;
        }
    }
}