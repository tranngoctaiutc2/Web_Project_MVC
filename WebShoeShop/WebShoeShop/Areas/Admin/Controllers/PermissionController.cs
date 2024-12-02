using System.Linq;
using System.Web.Mvc;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Areas.Admin.Controllers
{
	public class PermissionController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Admin/Permission
		public ActionResult Index()
		{
			var permissions = db.RolePermissions.ToList();
			return View(permissions);
		}
		public ActionResult Create()
		{
			var roles = db.Roles.ToList();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(RolePermission model)
		{
			if (ModelState.IsValid)
			{
				db.RolePermissions.Add(model);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			var roles = db.Roles.ToList();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(model);
		}


		public ActionResult Delete(int id)
		{
			var permission = db.RolePermissions.Find(id);
			if (permission == null)
			{
				return HttpNotFound();
			}
			return View(permission);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			var permission = db.RolePermissions.Find(id);
			if (permission != null)
			{
				db.RolePermissions.Remove(permission);
				db.SaveChanges();
			}
			return RedirectToAction("Index");
		}
	}
}