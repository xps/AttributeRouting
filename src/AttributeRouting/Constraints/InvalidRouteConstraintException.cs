using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AttributeRouting.Constraints
{
    public class InvalidRouteConstraintException : Exception
    {
        public InvalidRouteConstraintException(string message)
            : base(message) {}
    }
}
