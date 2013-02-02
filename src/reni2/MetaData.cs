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

namespace Reni
{
    public class MetaData
    {
        public static void DumpSearchPaths()
        {
            var ppts = Assembly
                .GetExecutingAssembly()
                .GetReferencedTypes()
                .SelectMany(t => t.GetInterfaces().Select(i => new {t, i}))
                .Where(ti => ti.i.IsGenericType && ti.i.GetGenericTypeDefinition() == typeof(ISearchPath<,>))
                .Select
                (
                    ti =>
                        new
                        {
                            provider = ti.i.GetGenericArguments()[1],
                            path = ti.i.GetGenericArguments()[0],
                            token = ti.t,
                            @interface = ti.i
                        })
                .Where(ppt => !ppt.provider.IsGenericParameter)
                .ToArray();

            var toImplement = ppts
                .Where(ppt => !ppt.provider.Implements(InterfaceType(ppt.path, ppt.token)))
                .GroupBy(ppt => ppt.token.IsSubclassOf(typeof(Defineable)))
                .ToDictionary
                (
                    gTokenClass => gTokenClass.Key,
                    gTokenClass => gTokenClass.GroupBy(ppt => ppt.provider)
                        .Select
                        (
                            gProvider => DumpSearchPathsGroup
                                (
                                    gProvider.Key,
                                    gProvider.Select(ppt => DumpInterface(ppt.provider, ppt.path, ppt.token)),
                                    gProvider.Select(ppt => DumpMembers(ppt.provider, ppt.path, ppt.token))
                                )
                        )
                        .Stringify("\n========================================\n")
                );

            var toRemove = ppts
                .Where(ppt => ppt.provider.Implements(InterfaceType(ppt.path, ppt.token)))
                .GroupBy(ppt => ppt.token)
                .Select
                (
                    g => DumpSearchPathsGroup
                        (
                            g.Key,
                            g.Select(ppt => ppt.@interface.PrettyName())
                        )
                )
                .Stringify("\n========================================\n");

            Tracer.FlaggedLine
                (
                    "\n" + toImplement[true]
                        + "\n\n\n---------------------------------------\nConverters:\n---------------------------------------\n"
                        + toImplement[false] 
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
        static string DumpSearchPathsGroup(System.Type key, IEnumerable<string> interfaces)
        {
            return key.PrettyName()
                + "\n"
                + interfaces.Stringify("\n")
                ;
        }
        static string DumpInterface(System.Type provider, System.Type path, System.Type token) { return ", " + InterfaceName(path, token); }
        static string InterfaceName(System.Type path, System.Type token) { return InterfaceType(path, token).PrettyName(); }
        static System.Type InterfaceType(System.Type path, System.Type token) { return typeof(IFeaturePath<,>).MakeGenericType(path, token); }

        static string DumpMembers(System.Type provider, System.Type path, System.Type token)
        {
            return path.PrettyName()
                + " "
                + InterfaceName(path, token)
                + ".Feature { get { return Extension.Feature("
                + token.Name
                + "Result); } }";
        }
    }

    interface IFeaturePath<out TPath, TTarget>
    {
        TPath Feature { get; }
    }
}