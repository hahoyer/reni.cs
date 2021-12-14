using System.Collections.Generic;

namespace ReniUI.Formatting
{
    interface IEditPieces
    {
        IEnumerable<Edit> Get(IEditPiecesConfiguration parameter);
    }
}