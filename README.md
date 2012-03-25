This version is based on the work of Tim McCall, found at https://github.com/mccalltd/AttributeRouting  

It brings a richer syntax that you can use in your route URL to specify constraints inline.


Inline constraints
===============================

You can use the following syntax to constraint the parameter to be of the required type:

    // Restrict the 'id' parameter to digits only
    // Restrict the 'name' parameter to any string with a maximum length of 50-
    [GET("my-url/{id:int}/{name:string(50)}")]

Here is the full list of built-in type restrictions:

    string(max-length), bool, int, long, float, double, decimal

You can also add a regular expression constraint by using the following syntax:

    // Restrict 'someParam' to letters only
    [GET("my-url/{someParam:regex(^[a-zA-Z]+$)}")


Extensibility
===============================

You can easily add your own type of constraints. For example, here's how to create a constraint for even numbers:

 1. Create a new class that implements `IRouteConstraint`:

        public class EvenRouteConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                var value = values[parameterName];
                if (value == null)
                    return true;

                int intValue;
                if (!int.TryParse(value.ToString(), out intValue))
                    return false;

                return intValue % 2 == 0;
            }
        }

 2. Register this constraint, somewhere in your `Application_Start`:

        using AttributeRouting.Constraints;
        ...
        
        protected void Application_Start()
        {
            RouteConstraintFactory.RegisterConstraintType("even", typeof(EvenRouteConstraint));
        }

 3. Use the constraint:

        // Restricts 'someParam' to be an even number
        [GET("my-url/{someParam:even}")]


Support for Enums
===============================

You can restrict a parameter to the values of an `enum`:

 1. Create an enum:

        public enum Colors
        {
            Red,
            Green,
            Blue
        }

 2. Register the new constraint type:

        RouteConstraintFactory.RegisterConstraintType("color", typeof(EnumRouteConstraint<Colors>));

 3. Apply the constraint to a URL parameter:

        // Restrict 'someParam' to values 'red', 'green' and 'blue'
        [GET("my-url/{someParam:color}")]
        
