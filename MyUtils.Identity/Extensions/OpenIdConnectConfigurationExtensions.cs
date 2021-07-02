using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;

namespace MyUtils.Identity.Extensions
{
    public static class OpenIdConnectConfigurationExtensions
    {
        /// <summary>
        /// Validate the Issuer, Audiences and Signing-keys of the access token and return the <see cref="ClaimsPrincipal"/>.
        /// Throw exceptions for invalid token
        /// </summary>
        /// <param name="oidcServerUrl"></param>
        /// <param name="accessToken"></param>
        /// <param name="openIdConnConfig"></param>
        /// <returns></returns>
        public static ClaimsPrincipal ValidateToken(this OpenIdConnectConfiguration openIdConnConfig, string oidcServerUrl, string accessToken)
        {
            TokenValidationParameters validationParameters =
                new TokenValidationParameters
                {
                    ValidIssuer = openIdConnConfig.Issuer,
                    //ValidAudiences = new[] { $"{oidcServerUrl}/resources" },

#if DEBUG
                    // Because in debug mode, we will have reverse-proxy to the identity STS server, the signature key will be different as the external URL's
                    // e.g. Issuer is Cloudflare URL while signature certificate is using localhost
                    // So the SignatureValidator need to be overriden as https://stackoverflow.com/a/54434999/4684232
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,
                    SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                    {
                        var jwt = new JwtSecurityToken(token);

                        return jwt;
                    },
#else
                    IssuerSigningKeys = openIdConnConfig.SigningKeys,
#endif
                };
            //validationParameters.SignatureValidator = (token, parameters) => r

            SecurityToken validatedToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var claims = handler.ValidateToken(accessToken, validationParameters, out validatedToken);
            return claims;
        }

    }
}
