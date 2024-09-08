namespace ReniUI;

sealed class SyntaxExceptionGuard : GuiExceptionGuard<Helper.Syntax>
{
	public SyntaxExceptionGuard(CompilerBrowser parent)
		: base(parent) { }

	protected override string GetTestCode(string folderName) => @$"
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace ReniUI.Generated.{folderName};

[UnitTest]
public sealed class Test : CompilerTest
{{
    protected override string Target => (SmbFile.SourceFolder / ""Test.reni"").String;

    protected override void Run()
    {{
        base.Run<()>()
}}
";

	public override Helper.Syntax OnException(Exception exception)
	{
		CreateDiscriminatingTest(exception);
		return default;
	}
}