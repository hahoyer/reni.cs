namespace Reni.Code.ReplaceVisitor
{
    sealed class ReplaceEnvironment : Base
    {
        readonly Context.ContextBase _target;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="target"></param>
        public ReplaceEnvironment(Context.ContextBase target)
        {
            _target = target;
        }

        public Context.ContextBase Target{get { return _target; }}

    }
}
