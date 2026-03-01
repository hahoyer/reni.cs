namespace ReniUI.Formatting;

static class SourcePartEditExtension
{
    [UsedImplicitly]
    static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;

    extension(IEnumerable<ISourcePartEdit> target)
    {
        internal IEnumerable<Edit> GetEditPieces()
        {
            var parameter = new EditPieceParameter();

            var sortedTargets = target
                .OrderBy(node => node.SourcePart.Position)
                .ThenBy(node => node.SourcePart.Length)
                .ToArray();
            return sortedTargets
                .SelectMany(part => part.GetEditPieces(parameter))
                .ToArray();
        }

        internal IEnumerable<ISourcePartEdit>
            Indent(int direction)
            => direction == 0? target : target.Select(node => node.Indent(direction)).ToArray();
    }

    extension(ISourcePartEdit target)
    {
        internal IEnumerable<Edit> GetEditPieces(IEditPiecesConfiguration parameter)
        {
            switch(target)
            {
                case IEditPieces spe:
                    return spe.Get(parameter).ToArray();

                default:
                    Dumpable.NotImplementedFunction(target, parameter);
                    return null;
            }
        }

        public ISourcePartEdit CreateIndent(int count)
            => count != 0 && target.IsIndentTarget? new IndentedSourcePartEdit(target, count) : target;
    }
}