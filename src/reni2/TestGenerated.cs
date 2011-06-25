﻿using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest;
using Reni.Runtime;

namespace Reni
{
    //[TestFixture]
    public sealed class TestGenerated
    {
        /// <summary>
        ///     Special test, will not work automatically.
        /// </summary>
        /// created 18.07.2007 01:29 on HAHOYER-DELL by hh
        [Test]
        public static void Exec()
        {
            var os = BitsConst.OutStream;
            BitsConst.OutStream = new OutStream();
            ReniTest.MainFunction();
#pragma warning disable 168
            var osNew = BitsConst.OutStream;
#pragma warning restore 168
            BitsConst.OutStream = os;
        }
    }
}