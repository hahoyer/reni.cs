using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class ExpressionFormatting : DependantAttribute
    {
        [UnitTest]
        public void WithComments()
        {
            const string Text = @"systemdata: 
{ 1 type instance     # comment
(
)

     #(aa  Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance();
   
    ! mutable FreePointer: Memory array_reference mutable;
 repeat: /\ ^ while() then(^ body(), repeat(^));

  aa)#

}; 

1=1 then 2 else 4;
3;

(Text('H') << 'allo') dump_print";
            var compiler = new Compiler(text: Text);
            var x = compiler.SourceSyntax.Reformat();
            var expected = @"systemdata:
{
    1 type instance     # comment
    
    ()
    
         #(aa  Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance();
       
        ! mutable FreePointer: Memory array_reference mutable;
     repeat: /\ ^ while() then(^ body(), repeat(^));
    
      aa)#
    
    
};

1 = 1 then 2 else 4;
3;
(Text ('H') << 'allo') dump_print";
            x = x.Replace("\r", "");
            expected= expected.Replace("\r", "");

            Tracer.Assert(x == expected, "\n" + Tracer.Dump(x) + "\n" + expected.NodeDump());
            Tracer.Assert(false);
        }

        [UnitTest]
        public void FromSourcePartSimple()
        {
            const string Text = @"1";
            var compiler = new Compiler(text: Text);
            var x = compiler.SourceSyntax.
                Reformat();
            Tracer.Assert(x == "1", x);
        }

        [UnitTest]
        public void FromSourcePart()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var x = compiler.SourceSyntax.Reformat();
            Tracer.Assert(x == "(1, 3, 4, 6)", x);
        }

        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string Text = @"( # Comment
1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var span = (compiler.Source + 2).Span(3);
            var x = compiler.Token(span);

            Tracer.Assert(x.SourcePart.Id == "# Comment\r", x.Dump);
        }
    }
}