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

using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using Microsoft.Owin.Host.SystemWeb;


namespace WebAppTestPacker
{
    public partial class _Default : Page
    {
        string authority = "https://login.windows.net";
        string resource = "https://management.core.windows.net/";
        string baseAddress = "https://management.azure.com";
        string redirectUri = "https://localhost:44301/Default.aspx";
        string tenant = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        string subscription = "b063f4bb-62dd-45c2-8bfd-36f982ed0242";
        string clientId = "1286aa04-872d-4b53-b51a-6d9e4b86f469";
        string clientSecret = "GpxK7TCbQthZDgtjMOeCpF4gd4IR27DbAsozAMH7pms=";

        string spToken = string.Empty;
        string userToken = string.Empty;
        string userCode = string.Empty;

        const char cr = '\r';

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Collections.Specialized.NameValueCollection nvc = Request.Params;
            if (nvc["code"] != null)
            {
                userCode = nvc["code"];
                Session["userCode"] = userCode;
            }

            if (userCode == string.Empty)
            {
                string authorizationUrl = string.Format("https://login.windows.net/{0}/oauth2/authorize?api-version=1.0&response_type=code&client_id={1}&resource={2}&redirect_uri={3}",
                    tenant, clientId, resource, redirectUri);
                Response.Redirect(authorizationUrl);
            }
            else
                userCode = Session["userCode"].ToString();
        }

        private async Task<string> GetAccessTokenSP()
        {
            var authenticationContext = new AuthenticationContext(string.Format("{0}/{1}", authority, tenant));
            var credential = new ClientCredential(clientId: clientId, clientSecret: clientSecret);
            var result = await authenticationContext.AcquireTokenAsync(resource: resource, clientCredential: credential);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            string token = result.AccessToken;
            return token;
        }

        private async Task<string> GetAccessTokenUser()
        {
            var authenticationContext = new AuthenticationContext(string.Format("{0}/{1}", authority, tenant));
            ClientCredential credential = new ClientCredential(clientId, clientSecret);
            var result = await authenticationContext.AcquireTokenByAuthorizationCodeAsync(userCode, new Uri(redirectUri), credential);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            string token = result.AccessToken;
            return token;
        }


        protected async void ButtonExecute_Click(object sender, EventArgs e)
        {
            if (spToken == string.Empty)
                spToken = await GetAccessTokenSP();
            Execute(spToken);
        }



        protected async void ButtonExecuteUser_Click(object sender, EventArgs e)
        {
            if (userToken == string.Empty)
                userToken = await GetAccessTokenUser();
            Execute(userToken);
        }

        private async void Execute(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.BaseAddress = new Uri(baseAddress);
                string statusCode = string.Empty;
                string content = string.Empty;

                if (DropDownListVerb.Text == "Post")
                    using (var response = await client.PostAsync(string.Format(TextCommand.Text, subscription), null))
                    {
                        statusCode = response.StatusCode.ToString();
                        content = await response.Content.ReadAsStringAsync();
                    }
                else
                    using (var response = await client.GetAsync(string.Format(TextCommand.Text, subscription)))
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