using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebShoeShop.Models;

namespace WebShoeShop.Common
{
	public class CustomAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (!httpContext.User.Identity.IsAuthenticated)
			{
				return false;
			}

			var userId = httpContext.User.Identity.GetUserId();
			var routeData = httpContext.Request.RequestContext.RouteData;
			var currentController = routeData.Values["controller"].ToString();
			var currentAction = routeData.Values["action"].ToString();

			using (var db = new ApplicationDbContext())
			{
				var userRoles = db.Users
					.Where(u => u.Id == userId)
					.SelectMany(u => u.Roles.Select(r => r.RoleId))
					.ToList();
				if (userRoles.Contains("2ce3f156-c6d0-4ff1-9013-f534b0cc2923"))
				{
					return true;
				}
				var hasPermission = db.RolePermissions.Any(rp =>
					userRoles.Contains(rp.RoleId) &&
					rp.Controller == currentController &&
					(string.IsNullOrEmpty(rp.Action) || rp.Action == currentAction));

				return hasPermission;
			}
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			if (filterContext.HttpContext.Request.IsAjaxRequest())
			{
				filterContext.Result = new JsonResult
				{
					Data = new { success = false, message = "Bạn không có quyền truy cập vào chức năng này!" },
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
			}
			else
			{
				filterContext.Controller.TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
				filterContext.Result = new RedirectToRouteResult(
					new System.Web.Routing.RouteValueDictionary(new
					{
						area = "Admin",
						controller = "Home",
						action = "Index"
					})
				);
			}
		}
	}
}