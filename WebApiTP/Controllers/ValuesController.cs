using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Threading.Tasks;
using System.Security.Claims;
using WebApiTP.Utils;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace WebApiTP.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values
        public async Task<IEnumerable<string>> Get()
        {
            var myGroups = new List<Group>();
            var myDirectoryRoles = new List<DirectoryRole>();

            ClaimsIdentity claimsId = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var objectIds = await ClaimHelper.GetGroups(claimsId);
            await GraphHelper.GetDirectoryObjects(objectIds, myGroups, myDirectoryRoles);
            return myGroups.Select(d => d.DisplayName); 
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
