using System.Diagnostics;
using hw.UnitTest;
using Reni.FeatureTest.TypeType;

namespace ReniTest;

[UnitTest]
sealed class TypeNameExtenderTest
{
	[UnitTest]
	public void TestMethod()
	{
		InternalTest(typeof(int), "int");
		InternalTest(typeof(List<int>), "List<int>", "Generic.List<int>");
		InternalTest(typeof(List<List<int>>), "List<List<int>>", "Generic.List<Generic.List<int>>");
		InternalTest(typeof(Dictionary<int, string>), "Dictionary<int,string>", "Generic.Dictionary<int,string>");
		InternalTest(typeof(TypeOperator), "TypeType.TypeOperator");
	}

	[DebuggerHidden]
	static void InternalTest(Type type, params string[] expectedTypeNames)
		=> type
			.PrettyName()
			.In(expectedTypeNames)
			.Assert
				(() => type + "\nFound   : " + type.PrettyName() + "\nExpected: " + expectedTypeNames, 1);
}