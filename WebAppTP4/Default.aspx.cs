using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Security.Claims;


namespace WebAppTP4
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var ci = (System.Security.Claims.ClaimsIdentity)ClaimsPrincipal.Current.Identity;
            string userToken = ((System.IdentityModel.Tokens.BootstrapContext)ci.BootstrapContext).Token;


        }
    }
}