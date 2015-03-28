using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeshHouse.Stats.Web.SignalRHelpers
{
    public class AuthenticationMiddleware : OwinMiddleware
    {

        public AuthenticationMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async System.Threading.Tasks.Task Invoke(IOwinContext context)
        {
            var token = context.Request.Query.Get("Bearer");

            if (context.Request.Path.Value.StartsWith("/signalr"))
            {
                string bearerToken = context.Request.Query.Get("Bearer");
                if (bearerToken != null)
                {
                    string[] authorization = { "Bearer " + bearerToken };
                    context.Request.Headers.Add("Authorization", authorization);
                }
            }

            await Next.Invoke(context);
        }
    }
}