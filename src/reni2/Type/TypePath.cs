namespace Reni.Type
{
    internal class TypePath : ReniObject
    {
        public static TypePath AlignerInstance = new Aligner();
        public static TypePath ArrayInstance = new Array();
        public static TypePath EnableCutInstance = new EnableCut();
        public static TypePath RefInstance = new Ref();

        internal virtual TypePath TurnIntoSequence()
        {
            NotImplementedMethod();
            return null;
        }

        internal TypePath CombineWith(TypePath typePath)
        {
            if(typePath == null)
                return this;

            return VirtualCombineWith(typePath);
        }

        internal virtual TypePath VirtualCombineWith(TypePath typePath)
        {
            NotImplementedMethod(typePath);
            return null;
        }

        internal virtual TypePath TurnIntoRef()
        {
            NotImplementedMethod();
            return null;
        }

        private class Aligner : TypePath {}

        private class Array : TypePath
        {
            private readonly Sequence _sequence = new Sequence();

            internal override TypePath TurnIntoSequence()
            {
                return _sequence;
            }
        }

        private class EnableCut : TypePath {}

        private class Ref : TypePath
        {
            internal override TypePath VirtualCombineWith(TypePath typePath)
            {
                return typePath.TurnIntoRef();
            }
        }

        private class RefToSequence : TypePath {}

        private class Sequence : TypePath
        {
            private readonly TypePath _ref = new RefToSequence();

            internal override TypePath TurnIntoRef()
            {
                return _ref;
            }
        }
    }
}