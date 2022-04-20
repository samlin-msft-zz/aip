using Microsoft.Identity.Client;
using Microsoft.InformationProtection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;

namespace AIP_WebAPI.Common
{
    public class AuthDelegateImplementation : IAuthDelegate
    {
        //private static readonly string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance1"];
        private static readonly string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static readonly string clientId = ConfigurationManager.AppSettings["ida:ClientID"];
       // private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private static readonly string thumbprint = ConfigurationManager.AppSettings["ida:Thumbprint"];
        private static readonly bool doCertAuth = Convert.ToBoolean(ConfigurationManager.AppSettings["ida:DoCertAuth"]);
        private static readonly string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        //private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        //private string authority = aadInstance + tenantId;

        private ClaimsPrincipal _claimsPrincipal;

        public AuthDelegateImplementation(ClaimsPrincipal claimsPrincipal)
        {
            _claimsPrincipal = claimsPrincipal;
        }

        public string AcquireToken(Identity identity, string authority, string resource, string claim)
        {
            //Call method to get access token, providing the identity, authority, and resource.
            //Uses the claims principal provided to the contructor to get the bootstrap context
            var authResult = Task.Run(async () => await GetAccessTokenOnBehalfOfUser(authority, resource));
            return authResult.Result;
        }


        public async Task<string> GetAccessTokenOnBehalfOfUser(string authority, string resource)
        {
            IConfidentialClientApplication _app;

            AuthenticationResult result;

            if (doCertAuth)
            {
                // Read X509 cert from local store and build ClientAssertionCertificate.
                X509Certificate2 cert = Utilities.ReadCertificateFromStore(thumbprint);

                // Create confidential client using certificate.
                _app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                .WithRedirectUri(resource)
                                                .WithAuthority(authority)
                                                .WithCertificate(cert)
                                                .Build();
            }

            else
            {
                // Create confidential client using client secret.
                _app = ConfidentialClientApplicationBuilder.Create(clientId)
                                               .WithRedirectUri(resource)
                                               .WithAuthority(authority)
                                               .WithClientSecret(clientSecret)
                                               .Build();
            }

            // Store user access token of authenticated user.
            var ci = (ClaimsIdentity)_claimsPrincipal.Identity;
            string userAccessToken = (string)ci.BootstrapContext;


            // Generate a user assertion with the UPN and access token.            
            UserAssertion userAssertion = new UserAssertion(userAccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer");

            // Append .default to the resource passed in to AcquireToken().
            List<string> scopes = new List<string>() { resource[resource.Length - 1].Equals('/') ? $"{resource}.default" : $"{resource}/.default" };

            result = await _app.AcquireTokenOnBehalfOf(scopes, userAssertion)
              .ExecuteAsync();

            // Return the token to the API caller
            return (result.AccessToken);
        }
    }
}