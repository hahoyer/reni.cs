using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[Lists]
[UnitTest]
public sealed class TrainWreck : DependenceProvider
{
    [UnitTest]
    public void FunctionWithTrainWreck()
    {
        const string text = @"Text: @
  {
    value: ^.
  }
  result
";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void TwoWagons()
    {
        const string text = @"{
  first.

  system
  NewMemory.

  second.
}";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void TreeWagons()
    {
        const string text = @"{
  first.

  system
  NewMemory
  Attribute.

  second.
}";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void ComplexHead()
    {
        const string text = @"{
  first.

  (system parameters)
  NewMemory.

  second.
}";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void ShortCargo()
    {
        const string text = @"{
  first.

  (system parameters)
  NewMemory(cargo, 12)
  Attribute.

  second.
}";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void MediumCargo()
    {
        const string text = @"{
  first.

  (system parameters)
  NewMemory(cargo,cargo)
  Attribute.

  second.
}";
        const string expected = @"{
  first.

  (system parameters)
  NewMemory
    (cargo, cargo)
  Attribute.

  second.
}";

        text.SimpleFormattingTest(expected, 20);
    }

    [UnitTest]
    public void LongCargo()
    {
        const string text = @"{
  first.

  (system parameters)
  NewMemory(cargo,
12)
  Attribute.

  second.
}";
        const string expected = @"{
  first.

  (system parameters)
  NewMemory
  (
    cargo,
    12
  )
  Attribute.

  second.
}";

        text.SimpleFormattingTest(expected);
    }

    [UnitTest]
    public void TrainWreckInBrackets()
    {
        const string text = @"(
  ((^ elementType) * 1)
  array_reference
  mutable
)";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void WithOperator()
    {
        const string text = @"(
  systemdata
  FreePointer
  :=
    (systemdata FreePointer type)
    instance((result + count) mutable enable_reinterpretation)
)";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void EnoughIndent()
    {
        const string text = @"(
  first.

  data!public:
    system
    NewMemory
    (
    ).
)";
        text.SimpleFormattingTest();
    }
}