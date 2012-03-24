using System;
using System.Web;
using System.Web.Routing;

namespace AttributeRouting.Constraints
{
    /// <summary>
    /// Constraints a url parameter to be a string with a maximum length
    /// </summary>
    public class StringRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Maximum length of the string
        /// </summary>
        public int MaxLength { get; set; }

        public StringRouteConstraint(string maxLength)
        {
            int value;
            if (int.TryParse(maxLength, out value))
                this.MaxLength = value;
            else
                throw new InvalidRouteConstraintException("Invalid parameter for the 'string' constraint: " + maxLength);
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
                return true;

            return value.ToString().Length <= MaxLength;
        }
    }
}
