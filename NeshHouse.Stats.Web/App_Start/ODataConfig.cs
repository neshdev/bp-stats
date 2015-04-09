using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace NeshHouse.Stats.Web
{
    public static class ODataConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<User>("Users");
            builder.EntitySet<Connection>("Connections");
            builder.EntitySet<Group>("Groups");
            builder.EntitySet<UserGroup>("UserGroups");
            builder.EntitySet<Game>("Games");
            builder.EntitySet<GameResult>("GameResults");
            builder.EntitySet<Team>("TeamRatings");
            builder.EntitySet<UserTeam>("UserTeamRatings");
            builder.EntityType<UserGroup>().HasKey(x => new { x.UserName, x.GroupName });
            builder.EnableLowerCamelCase();

            config.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: builder.GetEdmModel());

            

        }
    }
}