using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using AttributeRouting.Framework;
using System.Reflection;

namespace AttributeRouting.Constraints
{
    public static class RouteConstraintFactory
    {
        // Constraint types cache
        private static Dictionary<string, Type> constraintTypes = new Dictionary<string, Type>();

        // Constraint instances cache, only for constraints that do not have any parameters
        private static Dictionary<string, IRouteConstraint> constraintInstances = new Dictionary<string, IRouteConstraint>();

        private static Type GetConstraintTypeFromCache(string constraintTypeName)
        {
            // Check in the cache first
            if (constraintTypes.ContainsKey(constraintTypeName))
                return constraintTypes[constraintTypeName];
            else
            {
                // Get the type that implements this constraint
                var type = Assembly.GetExecutingAssembly().GetType("AttributeRouting.Constraints." + constraintTypeName + "RouteConstraint", false, true);
                if (type == null)
                    throw new InvalidRouteConstraintException("Unknown constraint type: " + constraintTypeName);

                return constraintTypes[constraintTypeName] = type;
            }
        }

        private static IRouteConstraint GetConstraintInstanceFromCache(string constraintTypeName)
        {
            // Check in the cache first
            if (constraintInstances.ContainsKey(constraintTypeName))
                return constraintInstances[constraintTypeName];
            else
            {
                // Get the type that implements this constraint
                var type = GetConstraintTypeFromCache(constraintTypeName);

                // Create an instance of it
                var instance = (IRouteConstraint)Activator.CreateInstance(type);

                return constraintInstances[constraintTypeName] = instance;
            }
        }

        public static IRouteConstraint GetConstraint(string constraintTypeName, params string[] constraintParameters)
        {
            if (constraintParameters.Length == 0)
                return GetConstraintInstanceFromCache(constraintTypeName);

            try
            {
                return (IRouteConstraint)Activator.CreateInstance(GetConstraintTypeFromCache(constraintTypeName), constraintParameters);
            }
            catch (MissingMethodException)
            {
                // No constructor with that number of arguments
                throw new InvalidRouteConstraintException(
                    string.Format("Invalid parameter count ({0}) for the '{1}' constraint: {2}",
                        constraintParameters.Length,
                        constraintTypeName,
                        string.Join(",", constraintParameters)));
            }
        }

        public static void RegisterConstraintType(string constraintTypeName, Type type)
        {
            if (!typeof(IRouteConstraint).IsAssignableFrom(type))
                throw new ArgumentException("type must implement IRouteConstraint", "type");

            constraintTypes[constraintTypeName] = type;
        }
    }
}
