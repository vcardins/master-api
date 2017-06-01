using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.IdentityModel.Tokens;
using MasterApi.Web.Identity;

namespace MasterApi.Web.SignalR.Connections
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="IAuthorizeHubConnection" />
    /// <seealso cref="IAuthorizeHubMethodInvocation" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HubAuthorizeAttribute : Attribute, IAuthorizeHubConnection, IAuthorizeHubMethodInvocation
    {
        private readonly TokenProviderOptions _tokenOptions;
        private HttpRequest _request;

        public HubAuthorizeAttribute(TokenProviderOptions tokenOptions)
        {
            _tokenOptions = tokenOptions;
        }

        public bool AuthorizeHubConnection(HubDescriptor hubDescriptor, HttpRequest request)
        {
            _request = request;
            // authenticate by using bearer token in query string
            var token = request.Query["Bearer"];

            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = _tokenOptions.Audience,
                ValidIssuer = _tokenOptions.Issuer,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true
            };

            var handler = new JwtSecurityTokenHandler();
            var ticket = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            if (ticket?.Identity == null || !ticket.Identity.IsAuthenticated) return false;
            // set the authenticated user principal into environment so that it can be used in the future
            request.HttpContext.User = new ClaimsPrincipal(ticket.Identity);
            return true;
        }

        public virtual bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;
            // check the authenticated user principal from environment
            var principal = hubIncomingInvokerContext.Hub.Context.Request.HttpContext.User;
            if (principal?.Identity == null || !principal.Identity.IsAuthenticated) return false;
            // create a new HubCallerContext instance with the principal generated from token
            // and replace the current context so that in hubs we can retrieve current user identity
            hubIncomingInvokerContext.Hub.Context = new HubCallerContext(_request, connectionId);
            return true;
        }

       
    }
}