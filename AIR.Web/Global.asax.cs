using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using AIR.Commons;

namespace AIR.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public Guid SessionGuid { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Session_Start()
        {
            SessionGuid = Guid.NewGuid();
            Session[AIRResources.SessionGuid] = SessionGuid;
            Session[AIRResources.ZipPathList] = new List<string>();
            Session[AIRResources.iOs] = false;
            Session[AIRResources.Android] = false;
            Session[AIRResources.Windows] = false;
        }

        protected void Session_End()
        {
            // We delete all files
            var foldername = Session[AIRResources.SessionGuid].ToString();
            var folderPath = Path.Combine("~/App_Data", foldername);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }

    }
}
