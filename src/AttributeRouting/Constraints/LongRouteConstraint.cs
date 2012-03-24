using System;
using System.Web;
using System.Web.Routing;

namespace AttributeRouting.Constraints
{
    /// <summary>
    /// Constraints a url parameter to be a long
    /// </summary>
    public class LongRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
                return true;

            long dummy;

            return value is long || long.TryParse(value.ToString(), out dummy);
        }
    }
}
