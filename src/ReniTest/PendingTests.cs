//13:44:18.517 10.10.2020 ran 169 of 178 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
            TestRunner.RunTest(new ReniUI.Test.TokenLocating().CompoundSourcePart);
            TestRunner.RunTest(new ReniUI.Test.TokenLocating().CommentFromSourcePart);
            TestRunner.RunTest(new ReniUI.Test.TokenLocating().NamedCompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MoreMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().NotMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.ExpressionFormatting().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.ExpressionFormatting().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.ListMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().CombinationsOfMatching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().MixedMatching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().NestedMatching);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPosition);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInterAction3().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new ReniUI.Test.FormattingLong().ReformatPart);
}}}
