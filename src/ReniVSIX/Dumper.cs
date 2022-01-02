using System;
using System.Reflection;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Package;

namespace ReniVSIX
{
    static class Dumper
    {
        static bool RequestMethods(MemberInfo memberInfo, ParseRequest target)
        {
            switch(memberInfo.Name)
            {
                case nameof(target.Callback):
                case nameof(target.Text):
                case nameof(target.View):
                    return false;
            }

            var field = memberInfo as FieldInfo;
            if(field != null)
                return field.IsPublic;

            var property = memberInfo as PropertyInfo;
            if(property != null)
                return property.CanRead && property.GetMethod.IsPublic;

            Tracer.TraceBreak();
            return default;
        }

        [UsedImplicitly]
        static string Dump(Type type, ParseRequest target)
        {
            Tracer.TraceBreak();
            return default;
        }

        public static void Register()
            => Tracer.Dumper.Configuration.Handlers.Add<ParseRequest>(methodCheck: RequestMethods);
    }
}