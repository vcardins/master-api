using System;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using MasterApi.Core.Account.Models;
using MasterApi.Core.Auth.Services;
using MasterApi.Core.Infrastructure.Crypto;
using MasterApi.Core.Auth.Models;
using MasterApi.Core.Auth.Enums;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Web.Extensions;
using System.Linq;
using MasterApi.Core.Account.Enums;

namespace MasterApi.Web.Identity
{
    /// <summary>
    /// Token generator middleware component which is added to an HTTP pipeline.
    /// This class is not created by application code directly,
    /// instead it is added by calling the <see cref="TokenProviderAppBuilderExtensions.UseSimpleTokenProvider(IApplicationBuilder,TokenProviderOptions,RsaSecurityKey)"/>
    /// extension method.
    /// </summary>
    public class TokenProviderMiddleware
    {
        private HttpContext _context;
        private HttpResponse _httpResponse;
        private HttpRequest _httpRequest;

        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly ILogger _logger;

        private IAuthService _authService;
        private ICrypto _crypto;

        private Audience _audience;

        private const string AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProviderMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="options">The options.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<TokenProviderMiddleware>();
            _options = options.Value;

            ThrowIfInvalidOptions();

            _authService = serviceProvider.GetService(typeof (IAuthService)) as IAuthService;
            _crypto = serviceProvider.GetService(typeof(ICrypto)) as ICrypto;         
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            _context = context;
            _httpResponse = context.Response;
            _httpRequest = context.Request;

            // If the request path doesn't match, skip
            if (!_httpRequest.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                return _next(context);
            }

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!_httpRequest.Method.Equals("POST") || !_httpRequest.HasFormContentType)
            {
                return _httpResponse.ExecuteErrorAsync("Invalid request method or content type");
            }

            _logger.LogInformation("Handling request: " + _httpRequest.Path);

            return GenerateToken();
        }

        private async Task GenerateToken()
        {
            var form = _httpRequest.Form;

            var clientId = form["client_id"];
            var clientSecret = form["client_secret"];
            var refreshToken = form["refresh_token"];
            var grantType = form["grant_type"];
            var username = form["username"];
            var password = form["password"];

            String refreshTokenId = string.Empty;
            AuthResponse token = null;

            if (string.IsNullOrEmpty(grantType))
            {
                await ThrowAuthError("Grant type is invalid", "invalid_grant_type");
                return;
            }

            var errorMessage = await ValidateClientAuthentication(clientId, clientSecret);

            if (errorMessage != null)
            {
                await ThrowAuthError(errorMessage, "invalid_clientId");
                return;
            }

            switch (grantType)
            {
                case "password":

                    if (string.IsNullOrEmpty(username))
                    {
                        errorMessage = "Username is required";
                    }

                    if (string.IsNullOrEmpty(password))
                    {
                        errorMessage = "Password is required";
                    }

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        await ThrowAuthError(errorMessage, "invalid_grant");
                        return;
                    }

                    var identity = await _options.IdentityResolver(username, password);
                    if (identity == null)
                    {
                        await ThrowAuthError("Invalid username or password.", "invalid_grant");
                        return;
                    }
                    
                    refreshTokenId = await StoreRefreshToken(identity);
                    token = await GetToken(identity, refreshTokenId, clientId);

                    await _httpResponse.ExecuteResultAsync(token);
                    return;

                case "refresh_token":

                    if (string.IsNullOrEmpty(username))
                    {
                        await ThrowAuthError("The refresh token is a required parameter", "invalid_refresh_token");
                    }

                    token = await GetRefreshToken(refreshToken, clientId);

                    await _httpResponse.ExecuteResultAsync(token);
                    return;                    
            }

            await ThrowAuthError("Invalid grant type", "invalid_grant_type");
            return;
        }

        private async Task<AuthResponse> GetToken(ClaimsIdentity identity, string refreshTokenId, string clientId)
        {
            // https://www.iana.org/assignments/jwt/jwt.xhtml
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iss, _options.Issuer),
                new Claim(JwtRegisteredClaimNames.Exp, _options.Expiration.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, await _options.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, _options.IssuedAt.ToString(), ClaimValueTypes.Integer64)
            };

            claims.AddRange(identity.Claims);

            // Create the JWT security token and encode it.
            var securityToken = new JwtSecurityToken(
                _options.Issuer,
                _options.Audience,
                claims,
                _options.NotBefore,
                _options.Expiration,
                _options.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(securityToken);
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value ?? UserAccessLevel.User.ToString();

            return new AuthResponse
            {
                AccessToken = encodedJwt,
                ExpiresIn = (int)_options.ValidFor.TotalSeconds,
                Issued = _options.IssuedAt,
                Expires = _options.Expiration,
                TokenType = AuthenticationScheme,
                Username = identity.Name,
                RefreshToken = refreshTokenId,
                AsClientId = clientId,
                Role = role
            };            
        }

        private async Task<string> StoreRefreshToken(IIdentity identity)
        {
            var refreshTokenId = Guid.NewGuid().ToString("n");
            var refreshToken = new RefreshToken
            {
                Id = _crypto.Hash(refreshTokenId),
                ClientId = _audience.ClientId,
                Subject = identity.Name,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_audience.RefreshTokenLifeTime)),
                ObjectState = ObjectState.Added
            };

            // create metadata to pass on to refresh token provider
            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { "as:client_id", _audience.ClientId },
                    { "userName", identity.Name }
                })
            {
                IssuedUtc = refreshToken.IssuedUtc,
                ExpiresUtc = refreshToken.ExpiresUtc
            };

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), props, JwtBearerDefaults.AuthenticationScheme);
            var ticketBytes = new TicketSerializer().Serialize(ticket);
            refreshToken.ProtectedTicket = Convert.ToBase64String(ticketBytes);

            await _authService.AddRefreshToken(refreshToken);

            return refreshTokenId;
        }

        private async Task<AuthResponse> GetRefreshToken(string token, string clientId)
        {
            var errorId = "invalid_refresh_token";

            if (string.IsNullOrEmpty(token))
            {
                await ThrowAuthError("Refresh token is required", errorId);
            }

            var hashedTokenId = _crypto.Hash(token);
            var refreshToken = await _authService.FindRefreshToken(hashedTokenId);
            if (refreshToken == null)
            {
                await ThrowAuthError("Refresh token not found", errorId);
            }

            var ticketBytes = Convert.FromBase64String(refreshToken.ProtectedTicket);
            var ticket = new TicketSerializer().Deserialize(ticketBytes);

            var originalClient = ticket.Properties.Items["as:client_id"];
            if (originalClient != clientId)
            {
                await ThrowAuthError("Refresh token is issued to a different clientId.", errorId);
            }

            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(ticket.Principal.Identity);
            var newTicket = new AuthenticationTicket(new ClaimsPrincipal(newIdentity), ticket.Properties, JwtBearerDefaults.AuthenticationScheme);

            return await GetToken(newIdentity, token, clientId);
        }

        private async Task ThrowAuthError(string message, string type = "", HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (string.IsNullOrEmpty(type))
            {
                await _httpResponse.ExecuteResultAsync(new { ErrorDescription = message }, statusCode);
            } else
            {
                await _httpResponse.ExecuteResultAsync(new AuthError { Error = type, Description = message }, statusCode);
            }
        }
       
        private async Task<string> ValidateClientAuthentication(string clientId, string clientSecret)
        {
            _audience = null;

            if (clientId == null)
            {
                return "Client id is missing";
            }

            _audience = await _authService.FindClientAsync(clientId);

            if (_audience == null)
            {
                return $"Client '{clientId}' is not registered in the system.";
            }

            if (_audience.ApplicationType != ApplicationTypes.NativeConfidential)
            {
                return !_audience.Active ? "Client is inactive." : null;
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                return "Client secret should be sent from native application types";
            }

            if (clientSecret != _crypto.Hash(clientSecret))
            {
                return "Client secret is invalid.";
            }

            return !_audience.Active ? "Client is inactive." : null;
        }

        private static MobileInfo GetMobileInfo(IHeaderDictionary requestHeaders)
        {
            var deviceToken = requestHeaders["X-DeviceTokenId"];
            var userAgent = requestHeaders["User-Agent"];

            if (string.IsNullOrEmpty(deviceToken))
            {
                return null;
            }

            return new MobileInfo
            {
                Token = deviceToken,
                InstallationId = requestHeaders["X-InstallationId"],
                ObjectId = requestHeaders["X-ObjectId"]
                //Os = GetOS(clientInfo),
                //Version = clientInfo.OS.Major,
                //Device = clientInfo.Device.Family
            };
        }
    
        private void ThrowIfInvalidOptions()
        {
            if (string.IsNullOrEmpty(_options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (string.IsNullOrEmpty(_options.Issuer))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
            }

            if (string.IsNullOrEmpty(_options.Audience))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
            }

            if (_options.ValidFor == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }

            if (_options.IdentityResolver == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
            }

            if (_options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }

            if (_options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.JtiGenerator));
            }
        }
        
    }
}