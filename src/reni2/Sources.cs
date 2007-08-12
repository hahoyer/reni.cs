using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal sealed class Sources: Dumpable
    {
        private List<Source> _data = new List<Source>();

        public void Add(Source source)
        {
            _data.Add(source);
        }

        internal Files Files(NoCaseStringDictionary<Config> config)
        {
            Files result = new Files();
            for (int i = 0; i < _data.Count; i++)
                result |= _data[i].Files(config);
            return result;
        }
        
        internal List<Source> Data { get { return _data; } }

        internal void Add(Sources sources)
        {
            _data.AddRange(sources._data);
        }
    }
}