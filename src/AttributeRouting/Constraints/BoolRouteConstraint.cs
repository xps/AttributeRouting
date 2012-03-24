using System;
using System.Web;
using System.Web.Routing;

namespace AttributeRouting.Constraints
{
    /// <summary>
    /// Constraints a url parameter to be a bool
    /// </summary>
    public class BoolRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
                return true;

            bool dummy;

            return value is bool || bool.TryParse(value.ToString(), out dummy);
        }
    }
}
