// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;

namespace Reni
{
    public class T4Compiler
    {
        readonly string _text;
        readonly string _className;
        public T4Compiler(string text, string className = "Reni")
        {
            _text = text;
            _className = className;
        }
        public string Code()
        {
            const string fileName = "temptest.reni";
            var f = fileName.FileHandle();
            f.String = _text;
            var compiler = new Compiler(fileName, _className);
            return compiler.ExecutedCode;
        }
    }
}