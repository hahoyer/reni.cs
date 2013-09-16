#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;

namespace Reni.FeatureTest.DefaultOperations
{
    [TestFixture]
    public abstract class ApplyCompareOperator : CompilerTest
    {
        protected override string Target { get { return "(1" + Operator + "100)dump_print"; } }
        protected abstract string Operator { get; }
        protected abstract bool Result { get; }
        protected override string Output { get { return Result ? "-1" : "0"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(Add2Numbers)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    public sealed class Equal : ApplyCompareOperator
    {
        protected override string Operator { get { return "="; } }
        protected override bool Result { get { return false; } }
    }

    public sealed class NotEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<>"; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class GreaterThan : ApplyCompareOperator
    {
        protected override string Operator { get { return ">"; } }
        protected override bool Result { get { return false; } }
    }

    public sealed class LessThan : ApplyCompareOperator
    {
        protected override string Operator { get { return "<"; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class LessOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<="; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class GreaterOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return ">="; } }
        protected override bool Result { get { return false; } }
    }
}