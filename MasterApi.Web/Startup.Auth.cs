using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using MasterApi.Core.Account.Enums;
using MasterApi.Web.Identity;

namespace MasterApi
{
    public partial class Startup
    {
        private RsaSecurityKey _signingKey;
        private TokenProviderOptions _tokenOptions;
       
        private void ConfigAuth(IServiceCollection services)
        {
            // *** CHANGE THIS FOR PRODUCTION USE ***
            // Here, we're generating a random key to sign tokens - obviously this means
            // that each time the app is started the key will change, and multiple servers 
            // all have different keys. This should be changed to load a key from a file 
            // securely delivered to your application, controlled by configuration.
            //
            // See the RSAKeyUtils.GetKeyParameters method for an examle of loading from
            // a JSON file.
            var keyParams = RSAKeyUtils.GetKeyParameters(".config/rsaparams.json");

            // Create the key, and a set of token options to record signing credentials 
            // using that key, along with the other parameters we will need in the 
            // token controlller.
            _signingKey = new RsaSecurityKey(keyParams);

            _tokenOptions = new TokenProviderOptions
            {
                Audience = _appSettings.Auth.TokenAudience,
                Issuer = _appSettings.Auth.TokenIssuer,
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256Signature),
                IdentityResolver = GetIdentity
            };

            // Save the token options into an instance so they're accessible to the 
            services.AddSingleton(typeof(TokenProviderOptions), _tokenOptions);

            services.AddAuthentication(options =>
            {
                options.SignInScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            // Enable the use of an [Authorize("Bearer")] attribute on methods and classes to protect.
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthSchema, policy =>
                {
                    policy.AuthenticationSchemes.Add(AuthSchema);
                    policy.RequireAuthenticatedUser().Build();
                });
            });
        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            var request = HttpContextAccessor.HttpContext.Request;
            var form = request.Form;

            if (await _userService.AuthenticateAsync(username, password, out Task<ClaimsIdentity> claimsIdentity, out UserAccountAuthStatus failure))
            {
                return await claimsIdentity;
            }
            
            // Credentials are invalid, or account doesn't exist
            return null;

        }
    }
}
