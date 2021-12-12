using System;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;
using Reni;

namespace ReniUI.RestFul
{
    public sealed class Channel
    {
        sealed class ExecutionProhibitedException : Exception { }

        readonly ValueCache<CompilerBrowser> CompilerCache;
        readonly ValueCache<StringStream> ResultCache;
        string TextValue;

        public Channel()
        {
            ResultCache = new(GetResult);
            CompilerCache = new(CreateCompiler);
        }

        public string Text
        {
            get => TextValue;
            set
            {
                if(value == TextValue)
                    return;

                TextValue = value;
                CompilerCache.IsValid = false;
                ResultCache.IsValid = false;
            }
        }

        CompilerBrowser CreateCompiler()
            => CompilerBrowser.FromText(TextValue);

        StringStream GetResult() => CompilerCache.Value.Result;

        [PublicAPI]
        public Issue[] GetIssues() => CompilerCache.Value.Issues.Select(Issue.Create).ToArray();

        public void ResetResult() => ResultCache.IsValid = false;

        public string GetOutput()
        {
            if(GetIssues().Any())
                throw new ExecutionProhibitedException();
            return ResultCache.Value.Data;
        }

        public string GetUnexpectedErrors()
        {
            if(GetIssues().Any())
                return "";
            return ResultCache.Value.Log;
        }
    }
}