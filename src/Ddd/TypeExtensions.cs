using System;
using System.Linq;

namespace Ddd
{
    public static class TypeExtensions
    {
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            var theObjectType = typeof(object);
            while (toCheck != null && toCheck != theObjectType)
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic, out Type[] genericTypeArguments)
        {
            var theObjectType = typeof(object);
            while (toCheck != null && toCheck != theObjectType)
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    genericTypeArguments = toCheck.GetGenericArguments();
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            genericTypeArguments = new Type[0];
            return false;
        }

		public static bool IsInterfaceOf(this Type toCheck, Type interfaceType)
		{
			return toCheck.GetInterfaces().Any(i => i.IsSubclassOfRawGeneric(interfaceType));
		}
    }
}
