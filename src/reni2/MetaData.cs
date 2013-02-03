#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    public class MetaData
    {
        public static void DumpSearchPaths()
        {
            var ppts = Assembly
                .GetExecutingAssembly()
                .GetReferencedTypes()
                .SelectMany(t => t.GetDirectInterfaces().Select(i => new {Type = t, Interface = i}))
                .Where(ti => ti.Interface.GetGenericType() == typeof(ISearchPath<,>))
                .Select
                (
                    ti =>
                        new
                        {
                            provider = ti.Interface.GetGenericArguments()[1],
                            path = ti.Interface.GetGenericArguments()[0],
                            target = ti.Type,
                            @interface = ti.Interface
                        })
                .Where(ppt => !ppt.provider.IsGenericParameter)
                .ToDictionaryEx(ppt => ppt.provider.Is(InterfaceType(ppt.path, ppt.target)));

            var toImplement = ppts[false]
                .ToDictionaryEx(ppt => ppt.target.Is<Defineable>() || ppt.target.Is<TypeBase>());

            var toImplementString = toImplement[true]
                .GroupBy(ppt => ppt.provider)
                .Select
                (
                    gProvider => DumpSearchPathsGroup
                        (
                            gProvider.Key,
                            gProvider.Select(ppt => DumpInterface(ppt.provider, ppt.path, ppt.target)),
                            gProvider.Select(ppt => DumpMembers(ppt.provider, ppt.path, ppt.target))
                        )
                )
                .Stringify("\n========================================\n");

            var unknown = toImplement[false]
                .GroupBy(ppt => ppt.target)
                .Select
                (
                    g =>
                        g.Key.PrettyName()
                            + ("\n" + g.Select(ppt => ppt.@interface.PrettyName()).Stringify("\n")).Indent()
                            + "\n"
                )
                .Stringify("\n");

            var toRemove = ppts[true]
                .GroupBy(ppt => ppt.target)
                .Select
                (
                    g =>
                        g.Key.PrettyName()
                            + ("\n" + g.Select(ppt => ppt.@interface.PrettyName()).Stringify("\n")).Indent()
                            + "\n"
                )
                .Stringify("\n");

            Tracer.FlaggedLine
                (
                    "\n"
                        + toImplementString
                        + "\n\n\n---------------------------------------\nUnknown:\n---------------------------------------\n"
                        + unknown
                        + "\n\n\n---------------------------------------\nTo remove:\n---------------------------------------\n"
                        + toRemove);
        }

        static string DumpSearchPathsGroup(System.Type key, IEnumerable<string> interfaces, IEnumerable<string> members)
        {
            return key.PrettyName()
                + "\n---Interfaces:\n"
                + interfaces.Stringify("\n")
                + "\n---Members:\n"
                + members.Stringify("\n");
        }
        static string DumpInterface(System.Type provider, System.Type path, System.Type token) { return ", " + InterfaceName(path, token); }
        static string InterfaceName(System.Type path, System.Type token) { return InterfaceType(path, token).PrettyName(); }
        static System.Type InterfaceType(System.Type path, System.Type token) { return typeof(IFeaturePath<,>).MakeGenericType(path, token); }

        static string DumpMembers(System.Type provider, System.Type path, System.Type token)
        {
            return path.PrettyName()
                + " "
                + InterfaceName(path, token)
                + ".GetFeature(" + token.Name + " target) { return Extension.Feature("
                + token.Name
                + "Result); }";
        }
    }
}