using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AttributeRouting.Constraints
{
    /// <summary>
    /// Constraints a url parameter to be a value from an enum
    /// </summary>
    public class EnumRouteConstraint<T> : IRouteConstraint where T : struct
    {
        private readonly HashSet<string> enumNames;

        public EnumRouteConstraint()
        {
            enumNames = new HashSet<string>(Enum.GetNames(typeof(T)).Select(name => name.ToLowerInvariant()));
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var parameterValue = values[parameterName].ToString().ToLowerInvariant();

            // The parameter is optional
            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                if (route.Defaults.ContainsKey(parameterName) && route.Defaults[parameterName] == UrlParameter.Optional)
                    return true;
                else
                    return false;
            }

            return enumNames.Contains(parameterValue);
        }
    }
}
