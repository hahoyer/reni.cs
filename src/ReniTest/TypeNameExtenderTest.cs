using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.TypeType;

namespace ReniTest
{
    [UnitTest]
    sealed class TypeNameExtenderTest
    {
        [UnitTest]
        public void TestMethod()
        {
            InternalTest(typeof(int), "int");
            InternalTest(typeof(List<int>), "List<int>");
            InternalTest(typeof(List<List<int>>), "List<List<int>>");
            InternalTest(typeof(Dictionary<int, string>), "Dictionary<int,string>");
            InternalTest(typeof(TypeOperator), "TypeType.TypeOperator");
        }

        [DebuggerHidden]
        static void InternalTest(Type type, string expectedTypeName)
        {
            Tracer
                .Assert
                (
                    type.PrettyName() == expectedTypeName,
                    () =>
                        type + "\nFound   : " + type.PrettyName() + "\nExpected: "
                        + expectedTypeName,
                    1);
        }
    }
}