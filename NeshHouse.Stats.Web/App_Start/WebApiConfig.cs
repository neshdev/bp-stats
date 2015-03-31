using Microsoft.AspNet.SignalR;
using NeshHouse.Stats.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;

namespace NeshHouse.Stats.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();


            var settings = new JsonSerializerSettings
            {
                ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
                {
                    AssembliesToInclude =
                    {
                        typeof(User).Assembly
                    }
                }
            };

            //var settings = new JsonSerializerSettings();
            //settings.ContractResolver = new SignalRContractResolver();
            var serializer = JsonSerializer.Create(settings);
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

            //var jsonNetSerializer = new JsonNetSerializer(serializerSettings);
//            GlobalHost.DependencyResolver.Register(typeof(IJsonSerializer), () => jsonNetSerializer);

            //config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }

    public class SignalRContractResolver : IContractResolver
    {
        private readonly Assembly _assembly;
        private readonly IContractResolver _camelCaseContractResolver;
        private readonly IContractResolver _defaultContractSerializer;

        public SignalRContractResolver()
        {
            _defaultContractSerializer = new DefaultContractResolver();
            _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            _assembly = typeof(Connection).Assembly;
        }

        #region IContractResolver Members

        public JsonContract ResolveContract(Type type)
        {
            if (type.Assembly.Equals(_assembly))
                return _defaultContractSerializer.ResolveContract(type);

            return _camelCaseContractResolver.ResolveContract(type);
        }

        #endregion
    }

    public class FilteredCamelCasePropertyNamesContractResolver : DefaultContractResolver
    {
        public HashSet<Assembly> AssembliesToInclude { get; set; }
        public HashSet<Type> TypesToInclude { get; set; }

        public FilteredCamelCasePropertyNamesContractResolver()
        {
            AssembliesToInclude = new HashSet<Assembly>();
            TypesToInclude = new HashSet<Type>();
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            Type declaringType = member.DeclaringType;
            if (TypesToInclude.Contains(declaringType) || AssembliesToInclude.Contains(declaringType.Assembly))
                jsonProperty.PropertyName = ToCamelCase(jsonProperty.PropertyName);

            return jsonProperty;
        }

        string ToCamelCase(string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            var firstChar = value[0];
            if (char.IsLower(firstChar))
                return value;

            firstChar = char.ToLowerInvariant(firstChar);
            return firstChar + value.Substring(1);
        }
    }
}
