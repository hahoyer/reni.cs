namespace ReniLSP;

sealed class FormattingConfiguration : DumpableObject
{
	public bool LineBreaksBeforeListToken;
	public bool AdditionalLineBreaksForMultilineItems = true;
	public bool LineBreaksAtComplexDeclaration;
	public int? EmptyLineLimit;
	public int? MaxLineLength;
}