﻿Feature: Route Defaults

Background: 
	Given I generate the routes defined in the subject controllers

Scenario: Route default specified with an attribute
	When I fetch the routes for the RouteDefaults controller's Index action
	Then the default for "p1" is "variable"

Scenario: Route default specified inline
	When I fetch the routes for the RouteDefaults controller's InlineDefaults action
	Then the route url is "InlineDefaults/{hello}/{goodnight}"
	Then the default for "hello" is "sun"
	Then the default for "goodnight" is "moon"

Scenario: Optional parameters specified with a url parameter token
	When I fetch the routes for the RouteDefaults controller's Optionals action
	Then the route url is "Optionals/{p1}/{p2}/{p3}"
	 And the parameter "p1" is optional
	 And the parameter "p2" is optional
	 And the parameter "p3" is optional

Scenario: Multiple routes with different defaults
	When I fetch the routes for the RouteDefaults controller's MultipleRoutes action
	Then the route named "MultipleDefaults1" has a default for "p1" of "first"
	 And the route named "MultipleDefaults2" has a default for "p1" of "second"

Scenario: Mixing inline optional parameters and constraints
	When I fetch the routes for the RouteDefaults controller's OptionalsAndConstraints action
	Then the route url is "OptionalsAndConstraints/{p1}/{p2}/{p3}"
	 And the parameter "p1" is of type "int"
	 And the parameter "p2" is of type "int"
	 And the parameter "p3" is of type "int"
	 And the parameter "p1" is optional
	 And the parameter "p2" is optional
	 And the parameter "p3" is optional

Scenario: Mixing inline optional parameters and constraints with parameters
	When I fetch the routes for the RouteDefaults controller's OptionalsAndConstraintsWithParameters action
	Then the route url is "OptionalsAndConstraintsWithParameters/{p1}/{p2}/{p3}"
	 And the parameter "p1" is of type "string" and has a maximum length of 10
	 And the parameter "p2" is of type "string" and has a maximum length of 10
	 And the parameter "p3" is of type "string" and has a maximum length of 10
	 And the parameter "p1" is optional
	 And the parameter "p2" is optional
	 And the parameter "p3" is optional