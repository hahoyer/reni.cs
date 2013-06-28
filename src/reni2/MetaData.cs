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
            var toImplement = ProviderTargetPaths(false)
                .ToDictionaryEx(ppt => ppt.Target.Is<Defineable>() || ppt.Target.Is<TypeBase>());

            var toImplementString = toImplement[true]
                .GroupBy(ppt => ppt.Provider)
                .Select(DumpSearchPathsGroup)
                .Stringify("\n========================================\n");

            var unknown = toImplement[false]
                .GroupBy(ppt => ppt.Target)
                .Select(PrettyName)
                .Stringify("\n");

            var toRemove = ProviderTargetPaths(true)
                .GroupBy(ppt => ppt.Target)
                .Select(PrettyName)
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
        static string DumpSearchPathsGroup(IGrouping<System.Type, ProviderTargetPath> gProvider)
        {
            return gProvider.Key.PrettyName()
                   + "\n---Interfaces:\n"
                   + gProvider.Select(ppt => DumpInterface(ppt.Path, ppt.Target)).Stringify("\n")
                   + "\n---Members:\n"
                   + gProvider.Select(ppt => DumpMembers(ppt.Path, ppt.Target)).Stringify("\n");
        }

        static string PrettyName(IGrouping<System.Type, ProviderTargetPath> g)
        {
            return g.Key.PrettyName()
                   + ("\n" + g.Select(ppt => ppt.Interface.PrettyName()).Stringify("\n")).Indent()
                   + "\n";
        }

        static IEnumerable<ProviderTargetPath> ProviderTargetPaths()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetReferencedTypes()
                .SelectMany(t => t.GetDirectInterfaces().Select(i => new {Type = t, Interface = i}))
                .Where(ti => ti.Interface.GetGenericType() == typeof(ISearchPath<,>))
                .Select
                (
                    ti =>
                    new ProviderTargetPath
                    {
                        Provider = ti.Interface.GetGenericArguments()[1],
                        Path = ti.Interface.GetGenericArguments()[0],
                        Target = ti.Type,
                        Interface = ti.Interface
                    })
                .Where(ppt => !ppt.Provider.IsGenericParameter);
        }

        static IEnumerable<ProviderTargetPath> ProviderTargetPaths(bool isWellKnown)
        {
            return ProviderTargetPaths()
                .Where(ppt => isWellKnown == ppt.Provider.Is(InterfaceType(ppt.Path, ppt.Target)));
        }

        static string DumpInterface(System.Type path, System.Type token) { return ", " + InterfaceName(path, token); }
        static string InterfaceName(System.Type path, System.Type token) { return InterfaceType(path, token).PrettyName(); }
        static System.Type InterfaceType(System.Type path, System.Type token) { return typeof(IFeaturePath<,>).MakeGenericType(path, token); }

        static string DumpMembers(System.Type path, System.Type token)
        {
            return path.PrettyName()
                   + " "
                   + InterfaceName(path, token)
                   + ".GetFeature(" + token.Name + " target) { return Extension.Feature("
                   + token.Name
                   + "Result); }";
        }
    }

    sealed class ProviderTargetPath
    {
        public System.Type Provider;
        public System.Type Path;
        public System.Type Target;
        public System.Type Interface;
    }
}