using Microsoft.AspNet.SignalR;
using NeshHouse.Stats.Web.Helpers;
using NeshHouse.Stats.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace NeshHouse.Stats.Web
{
    public static class SignalrConfig
    {
        public static void Register(HttpConfiguration config)
        {
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

            var serializer = JsonSerializer.Create(settings);
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
        }
    }
}