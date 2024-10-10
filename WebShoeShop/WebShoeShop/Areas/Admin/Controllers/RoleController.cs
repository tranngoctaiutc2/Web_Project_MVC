using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebShoeShop.Models;

namespace WebShoeShop.Areas.Admin.Controllers
{
    [Authorize(Roles ="Admin")]
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
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Nếu không có id, trả về lỗi BadRequest
			}

			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
			var role = roleManager.FindById(id); // Tìm role trong cơ sở dữ liệu theo id

			if (role == null)
			{
				return HttpNotFound(); // Nếu không tìm thấy role, trả về trang lỗi
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
				return HttpNotFound(); // Nếu không tìm thấy role, trả về trang lỗi
			}

			return View(role); // Trả về View để hiển thị thông tin về role trước khi xóa
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(string id)
		{
			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
			var role = roleManager.FindById(id); // Tìm role theo id

			if (role != null)
			{
				roleManager.Delete(role); // Thực hiện xóa role khỏi cơ sở dữ liệu
			}

			return RedirectToAction("Index"); // Sau khi xóa, chuyển hướng về trang danh sách các role
		}

	}
}