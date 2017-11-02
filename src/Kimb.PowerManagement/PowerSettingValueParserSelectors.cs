using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Kimb.PowerManagement
{
    public static class PowerSettingValueParserSelectors
    {
        public static IPowerSettingValueParserSelector FromCurrentAssembly()
        {
            var callingMethod = GetCallingMethod();
            var declaringType = callingMethod.DeclaringType;
            Debug.Assert(declaringType != null, "declaringType != null");
            var assembly = declaringType.Assembly;

            return FromTypes(assembly.GetTypes);
        }

        public static IPowerSettingValueParserSelector FromTypes(Func<IEnumerable<Type>> typeRepository, Func<Type, IPowerSettingValueParserSelector> instantiator = null)
        {
            if (typeRepository == null)
                throw new ArgumentNullException("typeRepository");
            instantiator = instantiator ?? (t => (IPowerSettingValueParserSelector)Activator.CreateInstance(t));

            var types = typeRepository();
            var selectors = types.Where(t => typeof(IPowerSettingValueParserSelector).IsAssignableFrom(t)
                                             && t.GetConstructor(Type.EmptyTypes) != null)
                                 .Select(instantiator);

            var selector = new CompositePowerSettingValueParserSelector(selectors);

            return selector;
        }

        private static MethodBase GetCallingMethod()
        {
            return new StackFrame(1).GetMethod();
        }
    }
}