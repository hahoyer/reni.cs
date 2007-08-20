namespace Reni.Code
{
	/// <summary>
	/// Arg is is used as a placeholder.
	/// </summary>
	internal sealed class Arg: Base
	{
	    Size _size;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="size"></param>
        public Arg(Size size)
        {
            _size = size;
            StopByObjectId(-579);
        }

	    /// <summary>
        /// Size of instances
        /// </summary>
        public override Size Size { get { return _size; } }

	    /// <summary>
	    /// Visitor to replace parts of code
	    /// </summary>
	    /// <param name="actual"></param>
	    /// <returns></returns>
        public override Result VirtVisit<Result>(Visitor<Result> actual)
	    {
            return actual.Arg(this);
	    }

	}
}
