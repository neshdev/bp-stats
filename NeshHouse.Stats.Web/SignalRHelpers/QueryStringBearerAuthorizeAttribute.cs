using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeshHouse.Stats.Web.SignalRHelpers
{
    using System.Security.Claims;

    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.AspNet.SignalR.Owin;

    public class QueryStringBearerAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
        {
            return base.UserAuthorized(user);
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            var token = request.QueryString.Get("Bearer");
            var authenticationTicket = Startup.AuthServerOptions.AccessTokenFormat.Unprotect(token);

            if (authenticationTicket == null || authenticationTicket.Identity == null || !authenticationTicket.Identity.IsAuthenticated)
            {
                return false;
            }

            request.Environment["server.User"] = new ClaimsPrincipal(authenticationTicket.Identity);
            request.Environment["server.Username"] = authenticationTicket.Identity.Name;
            //request.GetHttpContext().User = new ClaimsPrincipal(authenticationTicket.Identity);
            var user = new ClaimsPrincipal(authenticationTicket.Identity);
            this.UserAuthorized(user);
            
            return true;
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;

            // check the authenticated user principal from environment
            var environment = hubIncomingInvokerContext.Hub.Context.Request.Environment;
            var principal = environment["server.User"] as ClaimsPrincipal;

            if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated)
            {
                // create a new HubCallerContext instance with the principal generated from token
                // and replace the current context so that in hubs we can retrieve current user identity
                hubIncomingInvokerContext.Hub.Context = new HubCallerContext(new ServerRequest(environment), connectionId);

                return true;
            }

            return false;
        }
    }
}