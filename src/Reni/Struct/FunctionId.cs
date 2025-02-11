#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

#endregion

namespace Reni.Struct;

sealed class FunctionId : DumpableObject
{
    public static FunctionId Getter(int index) => new FunctionId(index, true);
    public static FunctionId Setter(int index) => new FunctionId(index, false);

    internal readonly int Index;
    internal readonly bool IsGetter;

    FunctionId(int index, bool isGetter)
    {
        Index = index;
        IsGetter = isGetter;
    }
    public override string ToString() => Dump();
    protected override string Dump(bool isRecursive) => Index + "." + (IsGetter ? "get" : "set");
}