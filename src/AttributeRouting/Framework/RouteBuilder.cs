﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using AttributeRouting.Constraints;
using AttributeRouting.Helpers;

namespace AttributeRouting.Framework
{
    public class RouteBuilder
    {
        private readonly AttributeRoutingConfiguration _configuration;

        public RouteBuilder(AttributeRoutingConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _configuration = configuration;
        }

        public IEnumerable<AttributeRoute> BuildAllRoutes()
        {
            var routeReflector = new RouteReflector(_configuration);
            var routeSpecs = routeReflector.GenerateRouteSpecifications().ToList();
            var mappedSubdomains = routeSpecs.Where(s => s.Subdomain.HasValue()).Select(s => s.Subdomain).Distinct().ToList();

            foreach (var routeSpec in routeSpecs)
            {
                foreach (var route in Build(routeSpec))
                {
                    route.MappedSubdomains = mappedSubdomains;
                    yield return route;                    
                }
            }
        }

        private IEnumerable<AttributeRoute> Build(RouteSpecification routeSpec)
        {
            var route = new AttributeRoute(CreateRouteUrl(routeSpec),
                                           CreateRouteDefaults(routeSpec),
                                           CreateRouteConstraints(routeSpec),
                                           CreateRouteDataTokens(routeSpec),
                                           _configuration)
            {
                RouteName = CreateRouteName(routeSpec),
                Translations = CreateRouteTranslations(routeSpec),
                Subdomain = routeSpec.Subdomain
            };

            // Yield the default route first
            yield return route;

            // Then yield the translations
            if (route.Translations == null)
                yield break;

            foreach (var translation in route.Translations)
            {
                // Backreference the default route.
                translation.DefaultRoute = route;

                yield return translation;
            }
        }

        private string CreateRouteName(RouteSpecification routeSpec)
        {
            if (routeSpec.RouteName.HasValue())
                return routeSpec.RouteName;

            if (_configuration.AutoGenerateRouteNames)
            {
                var area = (routeSpec.AreaName.HasValue()) ? routeSpec.AreaName + "_" : null;
                return "{0}{1}_{2}".FormatWith(area, routeSpec.ControllerName, routeSpec.ActionName);
            }

            return null;
        }

        private string CreateRouteUrl(RouteSpecification routeSpec)
        {
            return CreateRouteUrl(routeSpec.RouteUrl, routeSpec.RoutePrefixUrl, routeSpec.AreaUrl, routeSpec.IsAbsoluteUrl);
        }

        private string CreateRouteUrl(string routeUrl, string routePrefix, string areaUrl, bool isAbsoluteUrl)
        {
            var detokenizedUrl = DetokenizeUrl(routeUrl);
            
            var urlParameterNames = GetUrlParameterContents(detokenizedUrl);
        
            // {controller} and {action} tokens are not valid
            if (urlParameterNames.Any(n => n.ValueEquals("controller")))
                throw new AttributeRoutingException("{controller} is not a valid url parameter.");
            if (urlParameterNames.Any(n => n.ValueEquals("action")))
                throw new AttributeRoutingException("{action} is not a valid url parameter.");

            // Explicitly defined area routes are not valid
            if (urlParameterNames.Any(n => n.ValueEquals("area")))
                throw new AttributeRoutingException(
                    "{area} url parameters are not allowed. Specify the area name by using the RouteAreaAttribute.");

            var urlBuilder = new StringBuilder(detokenizedUrl);

            // If this is not an absolute url, prefix with a route prefix or area name
            if (!isAbsoluteUrl)
            {
                var delimitedRouteUrl = routeUrl + "/";
  
                if (routePrefix.HasValue())
                {
                    var delimitedRoutePrefix = routePrefix + "/";
                    if (!delimitedRouteUrl.StartsWith(delimitedRoutePrefix))
                        urlBuilder.Insert(0, delimitedRoutePrefix);                    
                }

                if (areaUrl.HasValue())
                {
                    var delimitedAreaUrl = areaUrl + "/";
                    if (!delimitedRouteUrl.StartsWith(delimitedAreaUrl))
                        urlBuilder.Insert(0, delimitedAreaUrl);                    
                }
            }

            // If we are lowercasing routes, then lowercase everything but the route params
            if (_configuration.UseLowercaseRoutes)
            {
                for (var i = 0; i < urlBuilder.Length; i++)
                {
                    var c = urlBuilder[i];
                    if (Char.IsUpper(c))
                    {
                        urlBuilder[i] = Char.ToLower(c);
                    }
                    else if (c == '{')
                    {
                        while (urlBuilder[i] != '}' && i < urlBuilder.Length)
                            i++;
                    }
                }       
            }

            return urlBuilder.ToString().Trim('/');
        }

        private RouteValueDictionary CreateRouteDefaults(RouteSpecification routeSpec)
        {
            var defaults = new RouteValueDictionary
            {
                { "controller", routeSpec.ControllerName },
                { "action", routeSpec.ActionName }
            };

            var urlParameters = GetUrlParameterContents(routeSpec.RouteUrl);

            // Inspect the url for optional parameters, specified with a leading or trailing (or both) ?
            foreach (var parameter in urlParameters.Where(p => p.StartsWith("?") || p.EndsWith("?")))
            {
                var parameterName = parameter.Trim('?');

                if (parameterName.Contains(':'))
                    parameterName = parameterName.Substring(0, parameterName.IndexOf(':'));

                if (defaults.ContainsKey(parameterName))
                    continue;

                defaults.Add(parameterName, UrlParameter.Optional);
            }

            // Inline defaults
            foreach (var parameter in urlParameters.Where(p => p.Contains('=')))
            {
                var indexOfEquals = parameter.IndexOf('=');
                var parameterName = parameter.Substring(0, indexOfEquals);

                if (parameterName.Contains(':'))
                    parameterName = parameterName.Substring(0, parameterName.IndexOf(':'));

                if (defaults.ContainsKey(parameterName))
                    continue;

                var defaultValue = parameter.Substring(indexOfEquals + 1, parameter.Length - indexOfEquals - 1);
                defaults.Add(parameterName, defaultValue);
            }

            // Attribute-based defaults
            foreach (var defaultAttribute in routeSpec.DefaultAttributes)
            {
                if (defaults.ContainsKey(defaultAttribute.Key))
                    continue;

                defaults.Add(defaultAttribute.Key, defaultAttribute.Value);
            }

            return defaults;
        }

        private RouteValueDictionary CreateRouteConstraints(RouteSpecification routeSpec)
        {
            var constraints = new RouteValueDictionary();

            // Default constraints
            if (routeSpec.HttpMethods.Any())
                constraints.Add("httpMethod", new RestfulHttpMethodConstraint(routeSpec.HttpMethods));

            // Inline constraints
            foreach (var parameter in GetUrlParameterContents(routeSpec.RouteUrl).Where(p => p.Contains(":")).Select(p => p.Trim('?')))
            {
                var indexOfColumn = parameter.IndexOf(':');
                var parameterName = parameter.Substring(0, indexOfColumn);

                if (constraints.ContainsKey(parameterName))
                    continue;

                var constraintDefinition = parameter.Substring(indexOfColumn + 1, parameter.Length - indexOfColumn - 1);
                if (constraintDefinition.Contains('='))
                    constraintDefinition = constraintDefinition.Substring(0, constraintDefinition.IndexOf('='));

                IRouteConstraint constraint;

                if (Regex.IsMatch(constraintDefinition, @"^.*\(.*\)$"))
                {
                    // Constraint of the form "firstName:string(50)"
                    var indexOfOpenParen = constraintDefinition.IndexOf('(');
                    var constraintType = constraintDefinition.Substring(0, indexOfOpenParen);
                    var constraintParams = constraintDefinition.Substring(indexOfOpenParen + 1, constraintDefinition.Length - indexOfOpenParen - 2);
                    constraint = RouteConstraintFactory.GetConstraint(constraintType, constraintParams.SplitAndTrim(new[] { "/" }));
                }
                else
                    // Constraint of the form "id:int"
                    constraint = RouteConstraintFactory.GetConstraint(constraintDefinition);

                constraints.Add(parameterName, constraint);
            }

            // Attribute-based constraints
            foreach (var constraintAttribute in routeSpec.ConstraintAttributes)
            {
                if (constraints.ContainsKey(constraintAttribute.Key))
                    continue;

                constraints.Add(constraintAttribute.Key, constraintAttribute.Constraint);
            }

            var detokenizedUrl = DetokenizeUrl(CreateRouteUrl(routeSpec));
            var urlParameterNames = GetUrlParameterContents(detokenizedUrl);

            // Convention-based constraints
            foreach (var defaultConstraint in _configuration.DefaultRouteConstraints)
            {
                var pattern = defaultConstraint.Key;
                
                foreach (var urlParameterName in urlParameterNames.Where(n => Regex.IsMatch(n, pattern)))
                {
                    if (constraints.ContainsKey(urlParameterName))
                        continue;

                    constraints.Add(urlParameterName, defaultConstraint.Value);
                }
            }

            return constraints;
        }

        private RouteValueDictionary CreateRouteDataTokens(RouteSpecification routeSpec)
        {
            var dataTokens = new RouteValueDictionary
            {
                { "namespaces", new[] { routeSpec.ControllerType.Namespace } }
            };

            if (routeSpec.AreaName.HasValue())
            {
                dataTokens.Add("area", routeSpec.AreaName);
                dataTokens.Add("UseNamespaceFallback", false);
            }

            return dataTokens;
        }

        private static string DetokenizeUrl(string url)
        {
            var patterns = new List<string>
            {
                @"(?<=\{)\?", // leading question mark (used to specify optional param)
                @"\?(?=\})", // trailing question mark (used to specify optional param)
                @"\:(.*?)(\(.*?\))?((?=\})|(?=\?\}))", // inline constraint type and parameters
                @"=.*?(?=\})", // equals and value (used to specify inline parameter default value)
            };

            return Regex.Replace(url, String.Join("|", patterns), "");
        }

        private IEnumerable<AttributeRoute> CreateRouteTranslations(RouteSpecification routeSpec)
        {
            // If no translation provider, then get out of here.
            if (!_configuration.TranslationProviders.Any())
                yield break;

            // Merge all the culture names from the various providers.
            var cultureNames = _configuration.GetTranslationProviderCultureNames();

            // Built the route translations, 
            // choosing the first available translated route component from among the providers
            foreach (var cultureName in cultureNames)
            {
                string translatedRouteUrl = null,
                       translatedRoutePrefix = null,
                       translatedAreaUrl = null;

                foreach (var provider in _configuration.TranslationProviders)
                {
                    translatedRouteUrl = translatedRouteUrl ?? provider.TranslateRouteUrl(cultureName, routeSpec);
                    translatedRoutePrefix = translatedRoutePrefix ?? provider.TranslateRoutePrefix(cultureName, routeSpec);
                    translatedAreaUrl = translatedAreaUrl ?? provider.TranslateAreaUrl(cultureName, routeSpec);
                }

                if (translatedRouteUrl == null && translatedRoutePrefix == null && translatedAreaUrl == null)
                    continue;

                var translatedRoute =
                    new AttributeRoute(CreateRouteUrl(translatedRouteUrl ?? routeSpec.RouteUrl,
                                                      translatedRoutePrefix ?? routeSpec.RoutePrefixUrl,
                                                      translatedAreaUrl ?? routeSpec.AreaUrl,
                                                      routeSpec.IsAbsoluteUrl),
                                       CreateRouteDefaults(routeSpec),
                                       CreateRouteConstraints(routeSpec),
                                       CreateRouteDataTokens(routeSpec),
                                       _configuration)
                    {
                        CultureName = cultureName,
                    };

                translatedRoute.DataTokens.Add("cultureName", cultureName);

                yield return translatedRoute;
            }
        }

        private static List<string> GetUrlParameterContents(string url)
        {
            if (!url.HasValue())
                return new List<string>();

            return (from urlPart in url.SplitAndTrim(new[] { "/" })
                    from match in Regex.Matches(urlPart, @"(?<={).*(?=})").Cast<Match>()
                    select match.Captures[0].ToString()).ToList();
        }
    }
}