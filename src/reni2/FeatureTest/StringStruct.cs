namespace Reni.FeatureTest
{
    public class StrinStruct : CompilerTest
    {
        protected static string Definition()
        {
            return
                @"
String: /\
{
    elementType: arg;
    CreateObject: /\

}
";
        }

        public override void Run() { }

        public override string Target
        {
            get { return Definition() + "; " + InstanceCode + " dump_print"; }
        }

        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

}                                                                