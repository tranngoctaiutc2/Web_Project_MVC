using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Controllers
{
	[Authorize]
	public class ShoppingCartController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;

		public ShoppingCartController()
		{
		}

		public ShoppingCartController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
		{
			UserManager = userManager;
			SignInManager = signInManager;
		}

		public ApplicationSignInManager SignInManager
		{
			get
			{
				return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set
			{
				_signInManager = value;
			}
		}

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		// GET: ShoppingCart
		public ActionResult Index()
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null && cart.Items.Any())
			{
				foreach (var item in cart.Items)
				{
					var productSize = db.ProductSizes.FirstOrDefault(ps => ps.ProductId == item.ProductId && ps.Size == item.Size);
					if (productSize == null || productSize.Quantity < item.Quantity)
					{
						return View();
					}
				}
				ViewBag.CheckCart = cart;
			}
			return View();
		}

		public ActionResult VnpayReturn()
		{
			if (Request.QueryString.Count > 0)
			{
				string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
				var vnpayData = Request.QueryString;
				VnPayLibrary vnpay = new VnPayLibrary();

				foreach (string s in vnpayData)
				{
					//get all querystring data
					if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
					{
						vnpay.AddResponseData(s, vnpayData[s]);
					}
				}
				string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
				long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
				string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
				string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
				String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
				String TerminalID = Request.QueryString["vnp_TmnCode"];
				long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
				String bankCode = Request.QueryString["vnp_BankCode"];

				bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
				if (checkSignature)
				{
					if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
					{
						var itemOrder = db.Orders.FirstOrDefault(x => x.Code == orderCode);
						if (itemOrder != null)
						{
							itemOrder.StatusPayMent = 2;//đã thanh toán    
							db.Orders.Attach(itemOrder);
							db.Entry(itemOrder).State = System.Data.Entity.EntityState.Modified;
							db.SaveChanges();
							ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
						}
						//Thanh toan thanh cong
						ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
					}
					else
					{
						//Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
						ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý. Xin vui lòng thử lại";
					}

				}
			}
			return View();
		}


		public ActionResult CheckOut()
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null && cart.Items.Any())
			{
				ViewBag.CheckCart = cart;
			}
			return View();
		}

		public ActionResult Partial_Item_ThanhToan()
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null && cart.Items.Any())
			{
				return PartialView(cart.Items);
			}
			return PartialView();
		}

		public ActionResult Partial_Item_Cart()
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null && cart.Items.Any())
			{
				foreach (var item in cart.Items)
				{
					var productSizes = db.ProductSizes.Where(p => p.ProductId == item.ProductId && p.Quantity > 0)
						.Select(p => p.Size).ToList();
					item.AvailableSizes = productSizes;
					var stock = db.ProductSizes.FirstOrDefault(ps => ps.ProductId == item.ProductId && ps.Size == item.Size);
					item.AvailableStock = stock?.Quantity ?? 0;
				}
				ViewBag.CouponCode = cart.CouponCode;
				return PartialView(cart.Items);
			}
			return PartialView();
		}
		[HttpPost]
		public JsonResult ApplyCoupon(string couponCode)
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart == null || !cart.Items.Any())
			{
				return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" });
			}

			if (string.IsNullOrWhiteSpace(couponCode))
			{
				foreach (var item in cart.Items)
				{
					item.Discount = 0;
				}
				cart.CouponCode = null;
				cart.TotalDiscount = 0;
				Session["Cart"] = cart;

				return Json(new { success = false, message = "Không có mã giảm giá áp dụng!", totalDiscount = 0 });
			}

			var coupon = db.Coupons.FirstOrDefault(c => c.Code == couponCode && c.IsActive &&
														 c.StartDate <= DateTime.Now && c.ExpirationDate >= DateTime.Now);

			if (coupon == null)
			{
				foreach (var item in cart.Items)
				{
					item.Discount = 0;
				}
				cart.CouponCode = null;
				cart.TotalDiscount = 0;
				Session["Cart"] = cart;
				return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn!", totalDiscount = 0 });
			}

			decimal totalDiscount = 0;

			foreach (var item in cart.Items)
			{
				if (coupon.DiscountPercentage.HasValue)
				{
					item.Discount = (item.Price * coupon.DiscountPercentage.Value / 100) * item.Quantity;
					if (coupon.MaxDiscountAmount.HasValue && item.Discount > coupon.MaxDiscountAmount.Value)
					{
						item.Discount = coupon.MaxDiscountAmount.Value;
					}
				}
				else if (coupon.DiscountAmount.HasValue)
				{
					item.Discount = coupon.DiscountAmount.Value;
				}

				totalDiscount += item.Discount;
			}

			cart.CouponCode = couponCode;
			cart.TotalDiscount = totalDiscount;
			Session["Cart"] = cart;

			return Json(new { success = true, message = "Áp dụng mã giảm giá thành công!", totalDiscount = totalDiscount });
		}


		[HttpGet]
		public ActionResult GetStockQuantity(int? productId, int? size)
		{
			if (productId == null || size == null)
			{
				return Json(new { Success = false, Message = "Tham số không hợp lệ." }, JsonRequestBehavior.AllowGet);
			}

			try
			{
				using (var db = new ApplicationDbContext())
				{
					var productSize = db.ProductSizes.FirstOrDefault(ps => ps.ProductId == productId && ps.Size == size);
					if (productSize != null)
					{
						return Json(new { Success = true, Stock = productSize.Quantity }, JsonRequestBehavior.AllowGet);
					}
					return Json(new { Success = false, Message = "Không tìm thấy thông tin tồn kho." }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception ex)
			{
				return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}


		public ActionResult ShowCount()
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null)
			{
				return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Partial_CheckOut()
		{
			var user = UserManager.FindByNameAsync(User.Identity.Name).Result;
			if (user != null)
			{
				ViewBag.User = user;
			}
			return PartialView();
		}

		public ActionResult CheckOutSuccess()
		{
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CheckOut(OrderViewModel req)
		{
			var code = new { Success = false, Code = -1, Url = "" };
			if (ModelState.IsValid)
			{
				ShoppingCart cart = (ShoppingCart)Session["Cart"];
				if (cart != null)
				{
					//Order order = new Order();
					Models.EF.Order order = new Models.EF.Order();
					order.CustomerName = req.CustomerName;
					order.Phone = req.Phone;
					order.Address = req.Address;
					order.Email = req.Email;
					order.Status = 1;
					order.StatusPayMent = 1;
					cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
					{
						ProductId = x.ProductId,
						Quantity = x.Quantity,
						Price = x.Price,
						Size = x.Size
					}));
					order.CouponCode = cart.CouponCode;
					decimal totaldiscout = cart.TotalDiscount;
					order.TotalDiscount = totaldiscout;
					order.Quantity = cart.GetTotalQuantity();
					if (req.TypeShip == 1)
					{
						order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
					}
					else if (req.TypeShip == 2)
					{
						order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity) + 70000);
					}
					order.TotalAmount -= totaldiscout;
					order.TypePayment = req.TypePayment;
					order.TypeShip = req.TypeShip;
					order.CreatedDate = DateTime.Now;
					order.ModifiedDate = DateTime.Now;
					order.CreatedBy = req.Phone;
					if (User.Identity.IsAuthenticated)
						order.CustomerId = User.Identity.GetUserId();
					Random rd = new Random();
					order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
					//order.E = req.CustomerName;
					db.Orders.Add(order);
					db.SaveChanges();
					//send mail cho khachs hang
					var strSanPham = "";
					var thanhtien = decimal.Zero;
					var TongTien = decimal.Zero;
					var strPrice = "";
					var strProductName = "";
					foreach (var sp in cart.Items)
					{
						strSanPham += "<tr>";
						strSanPham += "<td style=\"border-bottom:1px solid #e8e8e8; border-collapse:collapse; padding:10px;\">" + sp.ProductName + "</td>";
						strSanPham += "<td style=\"color:#000; font-family:&#39;Roboto&#39;, Arial, Helvetica, sans-serif; border-bottom:1px solid #e8e8e8; border-collapse:collapse; font-size:13px; font-weight:normal; letter-spacing:0.5px; line-height:1.5; text-align:center; padding:10px; margin:0 0 0;\">"
							+ WebShoeShop.Common.Common.FormatNumber(sp.Price, 0) + "</td>";


						strSanPham += "<td style=\"color:#000; font-family:&#39;Roboto&#39;, Arial, Helvetica, sans-serif; border-bottom:1px solid #e8e8e8; border-collapse:collapse; font-size:13px; font-weight:normal; letter-spacing:0.5px; line-height:1.5; text-align:center; padding:10px; margin:0 0 0;\">"
							+ sp.Quantity + "</td>";

						strSanPham += "<td style =\"color:#000; font-family:&#39;Roboto&#39;, Arial, Helvetica, sans-serif; border-bottom:1px solid #e8e8e8; border-collapse:collapse; font-size:13px; font-weight:500; letter-spacing:0.5px; line-height:1.5; text-align:center; padding:10px; margin:0 0 0;\">"
							 + WebShoeShop.Common.Common.FormatNumber(sp.TotalPrice, 0) + "</td>";

						strSanPham += "</tr>";
						thanhtien += sp.Price * sp.Quantity;
						var product = db.Products.Include(p => p.ProductSize)
							.FirstOrDefault(p => p.Id == sp.ProductId);
						if (product != null)
						{
							product.ReduceQuantity(sp.Quantity, sp.Size);
							product.Quantity -= sp.Quantity;
							db.Entry(product).State = EntityState.Modified;
						}
						strPrice = WebShoeShop.Common.Common.FormatNumber(sp.TotalPrice, 0);
						strProductName = sp.ProductName;
					}
					db.SaveChanges();
					if (req.TypeShip == 1)
					{
						TongTien = thanhtien;
					}
					else if (req.TypeShip == 2)
					{
						TongTien = thanhtien + 70000;
					}
					TongTien -= totaldiscout;
					string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/invoice-1.html"));
					contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
					contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
					contentCustomer = contentCustomer.Replace("{{Gia}}", strPrice);
					contentCustomer = contentCustomer.Replace("{{TenSanPham}}", strProductName);
					contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
					contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
					contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
					contentCustomer = contentCustomer.Replace("{{Email}}", req.Email);
					contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
					contentCustomer = contentCustomer.Replace("{{ThanhTien}}", WebShoeShop.Common.Common.FormatNumber(thanhtien, 0));
					if (req.TypeShip == 1)
					{
						contentCustomer = contentCustomer.Replace("{{PhiVanChuyen}}", "0");
					}
					else if (req.TypeShip == 2)
					{
						contentCustomer = contentCustomer.Replace("{{PhiVanChuyen}}", WebShoeShop.Common.Common.FormatNumber(70000, 0));
					}
					contentCustomer = contentCustomer.Replace("{{TongTien}}", WebShoeShop.Common.Common.FormatNumber(TongTien, 0));
					WebShoeShop.Common.Common.SendMail("Double 2T-2Q Store", "Đơn hàng #" + order.Code, contentCustomer.ToString(), req.Email);

					string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
					contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
					contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
					contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
					contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
					contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
					contentAdmin = contentAdmin.Replace("{{Email}}", req.Email);
					contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", order.Address);
					contentAdmin = contentAdmin.Replace("{{ThanhTien}}", WebShoeShop.Common.Common.FormatNumber(thanhtien, 0));
					if (req.TypeShip == 1)
					{
						contentAdmin = contentAdmin.Replace("{{PhiVanChuyen}}", "0");
					}
					else if (req.TypeShip == 2)
					{
						contentAdmin = contentAdmin.Replace("{{PhiVanChuyen}}", WebShoeShop.Common.Common.FormatNumber(70000, 0));
					}

					contentAdmin = contentAdmin.Replace("{{TongTien}}", WebShoeShop.Common.Common.FormatNumber(TongTien, 0));
					WebShoeShop.Common.Common.SendMail("Double 2T-2Q Store", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);
					cart.ClearCart();
					code = new { Success = true, Code = req.TypePayment, Url = "" };
					if (req.TypePayment == 2)
					{
						var url = UrlPayment(req.TypePaymentVN, order.Code);
						code = new { Success = true, Code = req.TypePayment, Url = url };
					}
				}
			}
			return Json(code);
		}

		[HttpPost]
		public ActionResult AddToCart(int id, int quantity, int size)
		{

			var code = new { Success = false, msg = "", code = -1, Count = 0 };
			if (Request.IsAuthenticated == false)
			{
				code = new { Success = false, msg = "Yêu cầu đăng nhập mới được thêm vào giỏ hàng", code = -1, Count = 0 };
			}
			var db = new ApplicationDbContext();
			var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
			if (checkProduct != null)
			{
				ShoppingCart cart = (ShoppingCart)Session["Cart"];
				if (cart == null)
				{
					cart = new ShoppingCart();
				}
				ShoppingCartItem item = new ShoppingCartItem
				{
					ProductId = checkProduct.Id,
					ProductName = checkProduct.Title,
					CategoryName = checkProduct.ProductCategory.Title,
					Alias = checkProduct.Alias,
					Quantity = quantity,
					Size = size
				};
				if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
				{
					item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
				}
				item.Price = checkProduct.Price;
				if (checkProduct.PriceSale > 0)
				{
					item.Price = (decimal)checkProduct.PriceSale;
				}
				item.TotalPrice = item.Quantity * item.Price;
				cart.AddToCart(item, quantity);
				Session["Cart"] = cart;
				code = new { Success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, Count = cart.Items.Count };
			}
			return Json(code);
		}
		[HttpPost]
		public ActionResult Update(int id, int quantity, int size)
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null)
			{
				using (var db = new ApplicationDbContext())
				{
					var productSizes = db.ProductSizes.FirstOrDefault(x => x.ProductId == id && x.Size == size);
					if (productSizes != null && productSizes.Quantity < quantity)
					{
						return Json(new { Success = false, Message = "Số lượng yêu cầu vượt quá số lượng tồn kho" });
					}
				}
				cart.UpdateQuantity(id, quantity, size);
				Session["Cart"] = cart;
				return Json(new { Success = true });
			}
			return Json(new { Success = false });
		}


		[HttpPost]
		public ActionResult Delete(int id, int size)
		{
			var code = new { Success = false, msg = "", code = -1, Count = 0 };

			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null)
			{
				var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id && x.Size == size);
				if (checkProduct != null)
				{
					cart.Remove(id, size);
					code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
				}
			}
			return Json(code);
		}



		[HttpPost]
		public ActionResult DeleteAll()
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart != null)
			{
				cart.ClearCart();
				return Json(new { Success = true });
			}
			return Json(new { Success = false });
		}


		#region Thanh toán vnpay
		public string UrlPayment(int TypePaymentVN, string orderCode)
		{
			var urlPayment = "";
			var order = db.Orders.FirstOrDefault(x => x.Code == orderCode);
			//Get Config Info
			string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
			string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
			string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
			string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

			//Build URL for VNPAY
			VnPayLibrary vnpay = new VnPayLibrary();
			var Price = (long)order.TotalAmount * 100;
			vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
			vnpay.AddRequestData("vnp_Command", "pay");
			vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
			vnpay.AddRequestData("vnp_Amount", Price.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
			if (TypePaymentVN == 1)
			{
				vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
			}
			else if (TypePaymentVN == 2)
			{
				vnpay.AddRequestData("vnp_BankCode", "VNBANK");
			}
			else if (TypePaymentVN == 3)
			{
				vnpay.AddRequestData("vnp_BankCode", "INTCARD");
			}

			vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
			vnpay.AddRequestData("vnp_CurrCode", "VND");
			vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
			vnpay.AddRequestData("vnp_Locale", "vn");
			vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.Code);
			vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

			vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
			vnpay.AddRequestData("vnp_TxnRef", order.Code); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

			//Add Params of 2.1.0 Version
			//Billing

			urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
			//log.InfoFormat("VNPAY URL: {0}", paymentUrl);
			return urlPayment;
		}
		#endregion
	}
}