namespace Reni.Code.ReplaceVisitor
{
    sealed class ReplaceEnvironment : Base
    {
        readonly Context.Base _target;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="target"></param>
        public ReplaceEnvironment(Context.Base target)
        {
            _target = target;
        }

        public Context.Base Target{get { return _target; }}

    }
}
