﻿using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApiTP.Utils
{
    class GraphHelper
    {
        public static async Task<string> AcquireToken(string userObjectId)
        {
            ClientCredential credential = new ClientCredential(ConfigHelper.ClientId, ConfigHelper.AppKey);
            var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as System.IdentityModel.Tokens.BootstrapContext;
            string userName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn) != null ? ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn).Value : ClaimsPrincipal.Current.FindFirst(ClaimTypes.Email).Value;
            string userAccessToken = bootstrapContext.Token;
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            UserAssertion userAssertion = new UserAssertion(bootstrapContext.Token, "urn:ietf:params:oauth:grant-type:jwt-bearer", userName);
            AuthenticationContext authContext = new AuthenticationContext(ConfigHelper.AadInstance + tenantId, null);
            AuthenticationResult result = await authContext.AcquireTokenAsync(ConfigHelper.GraphResourceId, credential, userAssertion);
            return result.AccessToken;
        }

        public async static Task GetDirectoryObjects(List<string> objectIds, List<Group> groups, List<User> users)
        {
            List<DirectoryRole> roles = new List<DirectoryRole>();
            await GetDirectoryObjects(objectIds, groups, roles, users);
        }

        public async static Task GetDirectoryObjects(List<string> objectIds, List<Group> groups, List<DirectoryRole> roles)
        {
            List<User> users = new List<User>();
            await GetDirectoryObjects(objectIds, groups, roles, users);
        }

        public async static Task GetDirectoryObjects(List<string> objectIds, List<Group> groups, List<DirectoryRole> roles, List<User> users)
        {
            string userObjectId = ClaimsPrincipal.Current.FindFirst(Globals.ObjectIdClaimType).Value;
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            ActiveDirectoryClient graphClient = new ActiveDirectoryClient(new Uri(ConfigHelper.GraphResourceId+'/'+tenantId), async () => { return await GraphHelper.AcquireToken(userObjectId); });

            int batchCount = 0;
            List<Task<IBatchElementResult[]>> requests = new List<Task<IBatchElementResult[]>>();
            List<IReadOnlyQueryableSetBase<IDirectoryObject>> batch = new List<IReadOnlyQueryableSetBase<IDirectoryObject>>();
            System.Collections.IEnumerator idIndex = objectIds.GetEnumerator();
            System.Collections.IEnumerator nextId = objectIds.GetEnumerator();
            nextId.MoveNext();
            while (idIndex.MoveNext())
            {
                string thisId = idIndex.Current.ToString(); // for delegate capture
                batch.Add(graphClient.DirectoryObjects.Where(o => o.ObjectId.Equals(thisId)));
                batchCount++;
                if (!nextId.MoveNext() || batchCount == 5)
                {
                    requests.Add(graphClient.Context.ExecuteBatchAsync(batch.ToArray()));
                    batchCount = 0;
                    batch.Clear();
                }
            }

            IBatchElementResult[][] responses = await Task.WhenAll<IBatchElementResult[]>(requests);
            foreach (IBatchElementResult[] batchResult in responses)
            {
                foreach (IBatchElementResult query in batchResult)
                {
                    if (query.SuccessResult != null && query.FailureResult == null)
                    {
                        if (query.SuccessResult.CurrentPage.First() is Group)
                        {
                            Group group = query.SuccessResult.CurrentPage.First() as Group;
                            groups.Add(group);
                        }
                        if (query.SuccessResult.CurrentPage.First() is DirectoryRole)
                        {
                            DirectoryRole role = query.SuccessResult.CurrentPage.First() as DirectoryRole;
                            roles.Add(role);
                        }
                        if (query.SuccessResult.CurrentPage.First() is User)
                        {
                            User user = query.SuccessResult.CurrentPage.First() as User;
                            users.Add(user);
                        }
                    }
                    else
                    {
                        throw new Exception("Directory Object Not Found.");
                    }
                }
            }

            return;
        }
    }
}

