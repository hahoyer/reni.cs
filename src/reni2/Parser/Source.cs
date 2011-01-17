using System;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;

namespace Reni.Parser
{
    /// <summary>
    /// Source for compilation. Can be a file or only a string
    /// </summary>
    [Serializable]
    public class Source : ReniObject
    {
        private readonly string _data;
        private readonly File _file;

        /// <summary>
        /// ctor from file
        /// </summary>
        /// <param name="file"></param>
        public Source(File file)
        {
            _file = file;
            _data = _file.String;
        }

        /// <summary>
        /// natural indexer
        /// </summary>
        public char this[int index]
        {
            get
            {
                if(IsEnd(index))
                    return '\0';
                return _data[index];
            }
        }

        /// <summary>
        /// Checks if a position at or beyond end of source
        /// </summary>
        /// <param name="posn">the position</param>
        /// <returns></returns>
        public bool IsEnd(int posn)
        {
            return _data.Length <= posn;
        }

        /// <summary>
        /// Obtains a piece
        /// </summary>
        /// <param name="start">start position</param>
        /// <param name="length">number of characters</param>
        /// <returns></returns>
        public string SubString(int start, int length)
        {
            return _data.Substring(start, length);
        }

        /// <summary>
        /// creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="i">the caracter position in file</param>
        /// <param name="flagText">the flag text</param>
        /// <returns>the "FileName(LineNr,ColNr): flagText: " string</returns>
        public string FilePosn(int i, string flagText)
        {
            return Tracer.FilePosn(_file.FullName, LineNr(i), ColNr(i)+1, flagText);
        }

        private int LineNr(int iEnd)
        {
            var result = 0;
            for(var i = 0; i < iEnd; i++)
                if(_data[i] == '\n')
                    result++;
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

        /// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns>The file position of sourec file</returns>
        protected override string Dump(bool isRecursion)
        {
            return FilePosn(0, "see there");
        }
    }
}