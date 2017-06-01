using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using MasterApi.Web.Extensions;

namespace MasterApi.Web.Identity
{
    public static class TokenProviderAppBuilderExtensions
    {
        /// <summary>
        /// Uses the simple token provider.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="options">The options.</param>
        /// <param name="signingKey">The signing key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static IApplicationBuilder UseTokenProvider(this IApplicationBuilder app, TokenProviderOptions options, RsaSecurityKey signingKey)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }


            var tokenValidationParameters = new TokenValidationParameters
            {
                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = options.Audience,

                // When receiving a token, check that we've signed it. The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the token expiry
                ValidateLifetime = true,

                // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                // machines which should have synchronised time, this can be set to zero. Where external tokens are
                // used, some leeway here could be useful.
                ClockSkew = TimeSpan.FromMinutes(0)
            };

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                CookieName = "access_token",
                SlidingExpiration = true,
                TicketDataFormat = new CustomJwtDataFormat(SecurityAlgorithms.HmacSha256, tokenValidationParameters),
                Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.Value.StartsWith("/api"))
                        {
                            context.Response.Clear();
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return Task.FromResult(0);
                        }
                        context.Response.Redirect(context.RedirectUri);
                        return Task.FromResult(0);
                    }
                }
            });

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme,
                SaveToken = true,
                TokenValidationParameters = tokenValidationParameters,
                Events = new JwtBearerEvents
                {
                    OnChallenge = async context => {
                        
                        // Emit the WWW-Authenticate header.
                        context.Response.Headers.Append(HeaderNames.WWWAuthenticate, context.Options.Challenge);
                        context.Response.ContentType = "application/json";

                        var exceptionType = context.AuthenticateFailure.GetBaseException().GetType();

                        var description = context.ErrorDescription;
                        var error = context.Error;

                        if (exceptionType == typeof(SecurityTokenValidationException))
                        {
                            description = "The token is invalid";
                        }
                        else if (exceptionType == typeof(SecurityTokenInvalidIssuerException))
                        {
                            description = "The issuer is invalid";
                        }
                        else if (exceptionType == typeof(SecurityTokenExpiredException))
                        {
                            description = "The token has expired";
                        }
                        else if (exceptionType == typeof(SecurityTokenInvalidSignatureException))
                        {
                            description = "The toke signature is invalid";
                        }

                        await context.Response.ExecuteErrorAsync(description, error, HttpStatusCode.Unauthorized);

                        context.HandleResponse();
                    },
                }
            });

            return app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));
        }
    }
}
