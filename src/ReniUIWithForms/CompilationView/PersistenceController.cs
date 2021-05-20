using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ReniUI.CompilationView
{
    abstract class PersistenceController<T> : DumpableObject
    {
        readonly string _fileName;
        readonly T _data;
        readonly ValueCache<string> _savedDataCache;

        protected PersistenceController(string fileName, T data)
        {
            _fileName = fileName;
            _data = data;
            _savedDataCache = new ValueCache<string>(() => Serialize(_data));
        }

        internal T Data
        {
            get
            {
                _savedDataCache.IsValid = true;
                return _data;
            }
        }

        protected abstract string Serialize(T data);

        internal void CheckedSave()
        {
            if(Serialize(_data) == _savedDataCache.Value)
                return;

            _savedDataCache.IsValid = false;
            _fileName.ToSmbFile().String = _savedDataCache.Value;
        }
    }
}