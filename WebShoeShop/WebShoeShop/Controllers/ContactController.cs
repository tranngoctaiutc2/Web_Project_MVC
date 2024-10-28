using System;
using System.Web.Mvc;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Controllers
{
	public class ContactController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: Contact
		public ActionResult Index()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Index([Bind(Include = "Id,Name,Email,Message")] Contact contact)
		{
			if (ModelState.IsValid)
			{
				contact.CreatedDate = DateTime.Now;
				contact.ModifiedDate = DateTime.Now;
				db.Contacts.Add(contact);
				db.SaveChanges();
				TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
				return RedirectToAction("Index");
			}

			return View(contact);
		}
		// GET: Contact/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: Contact/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		/* [HttpPost]
		 [ValidateAntiForgeryToken]
		 public ActionResult Create([Bind(Include = "Id,Name,Email,Message,IsRead,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] Contact contact)
		 {
			 if (ModelState.IsValid)
			 {
				 db.Contacts.Add(contact);
				 db.SaveChanges();
				 return RedirectToAction("Index");
			 }

			 return View(contact);
		 }
 */
		// GET: Contact/Edit/5

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}
		public ActionResult FAQs()
		{
			return View();
		}
		public ActionResult Policy()
		{
			return View();
		}
		public ActionResult Security()
		{
			return View();
		}
		public ActionResult Franchisepolicy()
		{
			return View();
		}
		public ActionResult Career()
		{
			return View();
		}
		public ActionResult ContactMe()
		{
			return PartialView();
		}
	}
}
