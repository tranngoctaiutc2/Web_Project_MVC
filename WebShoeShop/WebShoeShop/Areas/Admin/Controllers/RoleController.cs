using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebShoeShop.Common;
using WebShoeShop.Models;

namespace WebShoeShop.Areas.Admin.Controllers
{
	[CustomAuthorize]
	public class RoleController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Admin/Role
		public ActionResult Index()
		{
			var items = db.Roles.ToList();
			return View(items);
		}
		public ActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(IdentityRole model)
		{
			if (ModelState.IsValid)
			{
				var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
				roleManager.Create(model);
				return RedirectToAction("Index");
			}
			return View(model);
		}
		public ActionResult Edit(string id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
			var role = roleManager.FindById(id);

			if (role == null)
			{
				return HttpNotFound();
			}

			return View(role);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(IdentityRole model)
		{
			if (ModelState.IsValid)
			{
				var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
				roleManager.Update(model);
				return RedirectToAction("Index");
			}
			return View(model);
		}
		[HttpGet]
		public ActionResult Delete(string id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Kiểm tra id có hợp lệ không
			}

			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
			var role = roleManager.FindById(id); // Tìm role theo id

			if (role == null)
			{
				return HttpNotFound();
			}

			return View(role);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(string id)
		{
			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
			var role = roleManager.FindById(id); // Tìm role theo id

			if (role != null)
			{
				roleManager.Delete(role);
			}

			return RedirectToAction("Index");
		}

	}
}