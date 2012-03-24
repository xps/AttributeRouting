using System;
using System.Web;
using System.Web.Routing;

namespace AttributeRouting.Constraints
{
    /// <summary>
    /// Constraints a url parameter to be a decimal
    /// </summary>
    public class DecimalRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
                return true;

            decimal dummy;

            return value is decimal || decimal.TryParse(value.ToString(), out dummy);
        }
    }
}
