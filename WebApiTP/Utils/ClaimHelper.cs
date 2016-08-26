using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

using Newtonsoft.Json;


namespace WebApiTP.Utils
{
    public class ClaimHelper
    {
        public static async Task<List<string>> GetGroups(ClaimsIdentity claimsId)
        {
            if (claimsId.FindFirst("_claim_names") != null
                && (Json.Decode(claimsId.FindFirst("_claim_names").Value)).groups != null)
                return await GetGroupsFromGraphAPI(claimsId);
            return claimsId.FindAll("groups").Select(c => c.Value).ToList();
        }

        private static async Task<List<string>> GetGroupsFromGraphAPI(ClaimsIdentity claimsIdentity)
        {
            // Acquire the Access Token
            ClientCredential credential = new ClientCredential(ConfigHelper.ClientId, ConfigHelper.AppKey);
            var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as System.IdentityModel.Tokens.BootstrapContext;
            string userName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn) != null ? ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn).Value : ClaimsPrincipal.Current.FindFirst(ClaimTypes.Email).Value;
            string userAccessToken = bootstrapContext.Token;
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            UserAssertion userAssertion = new UserAssertion(bootstrapContext.Token, "urn:ietf:params:oauth:grant-type:jwt-bearer", userName);
            AuthenticationContext authContext = new AuthenticationContext(ConfigHelper.AadInstance + tenantId, null);
            AuthenticationResult result = await authContext.AcquireTokenAsync(ConfigHelper.GraphResourceId, credential, userAssertion);

            // Get the GraphAPI Group Endpoint for the specific user from the _claim_sources claim in token
            string groupsClaimSourceIndex = (Json.Decode(claimsIdentity.FindFirst("_claim_names").Value)).groups;
            var groupClaimsSource = (Json.Decode(claimsIdentity.FindFirst("_claim_sources").Value))[groupsClaimSourceIndex];
            string requestUrl = groupClaimsSource.endpoint + "?api-version=" + ConfigHelper.GraphApiVersion;

            // Prepare and Make the POST request
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            StringContent content = new StringContent("{\"securityEnabledOnly\": \"false\"}");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
            HttpResponseMessage response = await client.SendAsync(request);

            JsonReturn jr = default(JsonReturn);
            // Endpoint returns JSON with an array of Group ObjectIDs
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                jr = JsonConvert.DeserializeObject<JsonReturn>(responseContent);
            }
            else
            {
                throw new WebException();
            }

            return jr.value;
        }
    }

    class JsonReturn
    {
        [JsonProperty("odata.metadata")]
        public string meta { get; set; }

        [JsonProperty("value")]
        public List<string> value { get; set; }

    }

}
