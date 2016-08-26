using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

using System.IdentityModel.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;



namespace WebAppTestPacker3
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        //private static string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string certThumbprint = ConfigurationManager.AppSettings["ida:CertThumbprint"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string resource = ConfigurationManager.AppSettings["ida:Resource"];
        private static string domain = ConfigurationManager.AppSettings["ida:Domain"];

        private static string webApiResource = ConfigurationManager.AppSettings["ida:WebApiResource"];

        private static string authority = aadInstance + tenantId;

        private static X509Certificate2 cert = Utils.AppSettings.cert;
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    //PostLogoutRedirectUri = postLogoutRedirectUri, **Will be set dynamically later 
                    Resource = resource,
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthenticationFailed = (context) =>
                        {
                            //This section added to handle scenario where user logs in, but cancels consenting to rights to read directory profile
                            //Sometimes the Consent Framework doesn't kick in so this code is also executed in that sitiation. The user is redirected to the log out page
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            context.ProtocolMessage.RedirectUri = appBaseUrl + postLogoutRedirectUri;
                            context.HandleResponse();
                            context.Response.Redirect(context.ProtocolMessage.RedirectUri);
                            return System.Threading.Tasks.Task.FromResult(0);
                        },
                        RedirectToIdentityProvider = (context) =>
                        {
                            //Dynamically set RedirectUri & PostLogoutRedirectUri
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl + postLogoutRedirectUri;
                            //context.ProtocolMessage.Prompt = "admin_consent";
                            return System.Threading.Tasks.Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = async (context) =>
                       {
                           var code = context.Code;
                           //ClientCredential credential = new ClientCredential(clientId, clientSecret);
                           ClientAssertionCertificate credential = new ClientAssertionCertificate(clientId, cert);

                           //string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
                           var userObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                           string tenantId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                           //System.Security.Claims.ClaimsPrincipal claimsPrincipal = System.Security.Claims.ClaimsPrincipal.Current;

                           AuthenticationContext authContext = new AuthenticationContext(aadInstance + tenantId, new Utils.EFADALTokenCache($"{tenantId}:{userObjectId}"));
                           AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, resource);
                           result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, webApiResource);
                       }
                    },
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        //RoleClaimType = "groups",
                        //SaveSigninToken = true
                    }
                }
  
                );

            // This makes any middleware defined above this line run before the Authorization rule is applied in web.config
            app.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}
