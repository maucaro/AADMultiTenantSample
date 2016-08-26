using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Claims;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;





namespace WebAppTestPacker3
{
    public partial class _Default : Page
    {
        private static readonly string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        //private static readonly string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static readonly string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        //private static readonly string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"]; If using Client Secrets instead of Certificates
        private static readonly string domain = ConfigurationManager.AppSettings["ida:Domain"];
        private static readonly string resource = ConfigurationManager.AppSettings["ida:Resource"];

        private static readonly string webApiResource = ConfigurationManager.AppSettings["WebApiResource"];
        private static readonly string webApiValuesUrl = ConfigurationManager.AppSettings["WebApiValuesUrl"];
        private static readonly string subscriptionId = "76a8d17d-6a5a-4838-b1a3-1bc2b8d0b1d7";
        private static readonly string baseAddressRest = "https://management.azure.com";
        private static readonly char cr = '\r';

        private static X509Certificate2 cert = Utils.AppSettings.cert;


        protected void Page_Load(object sender, EventArgs e)
        {
            //var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as System.IdentityModel.Tokens.BootstrapContext;
            //ClaimsIdentity identity = ClaimsPrincipal.Current.Identities.First();
            //string userAccessToken = bootstrapContext.Token;
        }

        private async Task<string> GetAccessTokenSP()
        {
            var authenticationContext = new AuthenticationContext(aadInstance + domain);
            //var credential = new ClientCredential(clientId: clientId, clientSecret: clientSecret); If using Client Secrets instead of Certificates
            ClientAssertionCertificate credential = new ClientAssertionCertificate(clientId, cert);
            var result = await authenticationContext.AcquireTokenAsync(resource: resource, clientCertificate: credential);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            string token = result.AccessToken;
            return token;
        }

        private async Task<string> GetAccessTokenUser()
        {
            var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var userIdentifier = new UserIdentifier(userObjectId, UserIdentifierType.UniqueId);
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            var authenticationContext = new AuthenticationContext(aadInstance + tenantId, new Utils.EFADALTokenCache($"{tenantId}:{userObjectId}"));
            //var credential = new ClientCredential(clientId: clientId, clientSecret: clientSecret); If using Client Secrets instead of Certificates
            ClientAssertionCertificate credential = new ClientAssertionCertificate(clientId, cert);
            var result = await authenticationContext.AcquireTokenSilentAsync(resource, credential, userIdentifier);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            string token = result.AccessToken;
            return token;
        }

        private async Task<string> GetAccessTokenWebApi()
        {
            var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var userIdentifier = new UserIdentifier(userObjectId, UserIdentifierType.UniqueId);
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            var authenticationContext = new AuthenticationContext(aadInstance + tenantId, new Utils.EFADALTokenCache($"{tenantId}:{userObjectId}"));
            //ClientCredential credential = new ClientCredential(clientId: clientId, clientSecret: clientSecret);
            ClientAssertionCertificate credential = new ClientAssertionCertificate(clientId, cert);
            var result = await authenticationContext.AcquireTokenSilentAsync(webApiResource, credential, userIdentifier);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            string token = result.AccessToken;
            return token;
        }


        protected async void ButtonExecute_Click(object sender, EventArgs e)
        {
            string spToken = await GetAccessTokenSP();
            Execute(spToken, DropDownListVerb.Text, baseAddressRest + string.Format(TextCommand.Text, subscriptionId));
        }



        protected async void ButtonExecuteUser_Click(object sender, EventArgs e)
        {
            string userToken = await GetAccessTokenUser();
            Execute(userToken, DropDownListVerb.Text, baseAddressRest + string.Format(TextCommand.Text, subscriptionId));
        }

        protected async void ButtonExecuteAPI_Click(object sender, EventArgs e)
        {
            string apiToken = await GetAccessTokenWebApi();
            Execute(apiToken, "Get", webApiValuesUrl);

        }

        private async void Execute(string token, string verb, string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                string statusCode = string.Empty;
                string content = string.Empty;

                if (verb == "Post")
                    using (var response = await client.PostAsync(url, null))
                    {
                        statusCode = response.StatusCode.ToString();
                        content = await response.Content.ReadAsStringAsync();
                    }
                else
                    using (var response = await client.GetAsync(url))
                    {
                        statusCode = response.StatusCode.ToString();
                        content = await response.Content.ReadAsStringAsync();
                    }
                TextBoxResult.Text = "Status Code: " + statusCode;
                TextBoxResult.Text += cr + "Content: " + cr + content;

            }

        }

    }
}