using System;
using System.Web;
using System.Web.Routing;

namespace AttributeRouting.Constraints
{
    /// <summary>
    /// Constraints a url parameter to be an int
    /// </summary>
    public class IntRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
                return true;

            int dummy;

            return value is int || int.TryParse(value.ToString(), out dummy);
        }
    }
}
