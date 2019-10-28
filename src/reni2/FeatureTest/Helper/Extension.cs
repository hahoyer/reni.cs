namespace Reni.FeatureTest.Helper
{
    static class Extension
    {
        public static void AssertSyntaxIsLike(this Compiler compiler, LikeSyntax prototype)
            => prototype.AssertLike(compiler.Syntax.Left.Right);
    }
}