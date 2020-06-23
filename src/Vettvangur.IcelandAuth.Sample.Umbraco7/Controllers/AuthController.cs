using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using umbraco.BusinessLogic.Actions;
using Umbraco.Web.Mvc;

namespace Vettvangur.IcelandAuth.Sample.Umbraco7.Controllers
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
