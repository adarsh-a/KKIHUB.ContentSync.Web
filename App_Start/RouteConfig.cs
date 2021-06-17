using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KKIHUB.ContentSync.Web
{
	public class RouteConfig /*: System.Web.HttpApplication*/
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
			  name: "syncupdated",
			  url: "api/content/syncupdated",
			   defaults: new { controller = "Content", action = "SyncContentUpdated" }
		  );
			
			routes.MapRoute(
			  name: "PushContent",
			  url: "api/content/pushcontent",
			   defaults: new { controller = "Content", action = "PushContent" }
		  );

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);


		}
		/*protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			//UnityConfig.RegisterComponents();
		}*/
	}
}
