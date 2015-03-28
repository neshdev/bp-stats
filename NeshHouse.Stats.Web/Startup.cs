using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(NeshHouse.Stats.Web.Startup))]
namespace NeshHouse.Stats.Web
{
    public class Startup
    {
        public static OAuthAuthorizationServerOptions AuthServerOptions;

        static Startup()
        {
            AuthServerOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider(),
            };
        }

        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

        public void Configuration(IAppBuilder app)
        {

            HttpConfiguration config = new HttpConfiguration();

            ConfigureOuth(app);

            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);


            var hubConfig = new HubConfiguration();
            hubConfig.EnableDetailedErrors = true;
            app.MapSignalR(hubConfig);
        }

        public void ConfigureOuth(IAppBuilder app)
        {
            app.UseExternalSignInCookie(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);

            //OAuthAuthorizationServerOptions OAuthServerOption = new OAuthAuthorizationServerOptions()
            //{
            //    AllowInsecureHttp = true,
            //    TokenEndpointPath = new PathString("/token"),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
            //    Provider = new SimpleAuthorizationServerProvider(),
            //};

            //this is to provide the token
            app.UseOAuthAuthorizationServer(AuthServerOptions);

            //this authenticating the token
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
                {
                    Provider = new ApplicationOAuthBearerAuthenticationProvider(),
                });

        }
    }
}