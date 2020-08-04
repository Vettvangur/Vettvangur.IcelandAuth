using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;

namespace Vettvangur.IcelandAuth.Sample.Umbraco8.Controllers
{
    public class AuthController : SurfaceController
    {
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToCurrentUmbracoPage();
        }
    }
}
