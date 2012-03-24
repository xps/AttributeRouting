using System.Linq;
using AttributeRouting.Constraints;
using AttributeRouting.Framework;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AttributeRouting.Specs.Steps
{
    [Binding]
    public class RouteConstraintSteps
    {
        [Then(@"the parameter ""(.*?)"" is constrained by the pattern ""(.*?)""")]
        public void ThenTheParameterIsContrainedBy(string key, object pattern)
        {
            var route = ScenarioContext.Current.GetFetchedRoutes().First();

            Assert.That(route, Is.Not.Null);
            Assert.That(route.Constraints[key], Is.TypeOf(typeof(RegexRouteConstraint)));
            Assert.That(((RegexRouteConstraint)route.Constraints[key]).Pattern, Is.EqualTo(pattern));
        }

        [Then(@"the parameter ""(.*?)"" is of type ""(.*?)""")]
        public void ThenTheParameterIsOfType(string key, string type)
        {
            var route = ScenarioContext.Current.GetFetchedRoutes().First();
            var constraintType = typeof(AttributeRoute).Assembly.GetType("AttributeRouting.Constraints." + type + "RouteConstraint", true, true);

            Assert.That(route, Is.Not.Null);
            Assert.That(route.Constraints[key], Is.TypeOf(constraintType));
        }

        [Then(@"the parameter ""(.*?)"" is of type ""string"" and has a maximum length of (\d*?)")]
        public void ThenTheParameterIsOfType(string key, int maxLength)
        {
            var route = ScenarioContext.Current.GetFetchedRoutes().First();

            Assert.That(route, Is.Not.Null);
            Assert.That(route.Constraints[key], Is.TypeOf(typeof(StringRouteConstraint)));
            Assert.That(((StringRouteConstraint)route.Constraints[key]).MaxLength, Is.EqualTo(maxLength));
        }

        [Then(@"the route named ""(.*)"" has a constraint on ""(.*)"" of ""(.*)""")]
        public void ThenTheRouteNamedHasAConstraintOnOf(string routeName, string key, string value)
        {
            var route = ScenarioContext.Current.GetFetchedRoutes().Cast<AttributeRoute>().SingleOrDefault(r => r.RouteName == routeName);

            Assert.That(route, Is.Not.Null);

            var constraint = route.Constraints[key];

            Assert.That(constraint, Is.Not.Null);
            Assert.That(constraint, Is.TypeOf(typeof(RegexRouteConstraint)));
            Assert.That(((RegexRouteConstraint)route.Constraints[key]).Pattern, Is.EqualTo(value));
        }
    }
}
