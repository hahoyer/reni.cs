using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;

namespace Reni.Parser
{
    public sealed class Source : ReniObject
    {
        private readonly string _data;
        private readonly File _file;

        internal Source(File file)
        {
            _file = file;
            _data = _file.String;
        }

        internal Source(string data) { _data = data; }

        internal char this[int index]
        {
            get
            {
                if(IsEnd(index))
                    return '\0';
                return _data[index];
            }
        }

        internal bool IsEnd(int posn) { return _data.Length <= posn; }

        internal string SubString(int start, int length) { return _data.Substring(start, length); }

        internal string FilePosn(int i, string flagText)
        {
            if(_file == null)
                return "????";
            return Tracer.FilePosn(_file.FullName, LineNr(i), ColNr(i) + 1, flagText);
        }

        private int LineNr(int iEnd)
        {
            var result = 0;
            for(var i = 0; i < iEnd; i++)
            {
                if(_data[i] == '\n')
                    result++;
            }
            return result;
        }

        private int ColNr(int iEnd)
        {
            var result = 0;
            for(var i = 0; i < iEnd; i++)
            {
                result++;
                if(_data[i] == '\n')
                    result = 0;
            }
            return result;
        }

        protected override string Dump(bool isRecursion) { return FilePosn(0, "see there"); }
    }
}