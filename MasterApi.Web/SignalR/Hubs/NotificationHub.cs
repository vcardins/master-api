using MasterApi.Core.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MasterApi.Web.SignalR.Hubs
{
    [HubName("notification")]
    public class NotificationHub : Hub
    {
        private readonly IUserProfileService _userProfileService;
        public static string ConnectionId;
        protected static readonly ConcurrentDictionary<string, List<string>> UserConnectionIds = new ConcurrentDictionary<string, List<string>>();

        public NotificationHub(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        public void NewUpdate(string command, double state)
        {
            Clients.Others.newUpdate(command, state);
        }

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
        public override Task OnConnected()
        {
            Connect();
            return base.OnConnected();
        }

        /// <summary>
        /// Called when the connection reconnects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
        public override Task OnReconnected()
        {
            Connect(true);
            return base.OnReconnected();
        }

        /// <summary>
        /// Connects the specified is reconnect.
        /// </summary>
        /// <param name="isReconnect">if set to <c>true</c> [is reconnect].</param>
        /// <returns></returns>
        public Task Connect(bool isReconnect = false)
        {
            var connectionId = Context.ConnectionId;
            // check the authenticated user principal from environment
            //var environment = Context.Request.Environment;
            //var principal = environment["server.User"] as ClaimsPrincipal;
            //if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated)
            //{
            //    // create a new HubCallerContext instance with the principal generated from token
            //    // and replace the current context so that in hubs we can retrieve current user identity
            //    Context = new HubCallerContext(new HttpRequest(environment), connectionId);
            //}


            //Clients.Caller.handleEvent(evt);

            return null;
        }
    }
}
