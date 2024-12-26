using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Controllers
{
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
					var itemOrder = db.Orders.FirstOrDefault(x => x.Code == orderCode);
					if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
					{

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
						itemOrder.Status = 0;
						foreach (var detail in itemOrder.OrderDetails)
						{
							var product = db.Products
								.Include(p => p.ProductSize)
								.FirstOrDefault(p => p.Id == detail.ProductId);

							if (product != null)
							{
								// Hoàn lại số lượng sản phẩm
								product.ReturnQuantity(detail.Quantity, (int)detail.Size);
								product.Quantity += detail.Quantity;
								product.SoldQuantity -= detail.Quantity;
								db.Entry(product).State = EntityState.Modified;
							}
						}
						db.SaveChanges();
						//Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
						ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý. Xin vui lòng thử lại";
					}
				}
				else
				{
					ViewBag.InnerText = "Chữ ký không hợp lệ";
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
		public JsonResult ApplyCouponCode(string couponCode)
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
				return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn!", totalDiscount = 0 }, JsonRequestBehavior.AllowGet);
			}
			if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
			{
				return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng." });
			}
			decimal orderTotal = cart.Items.Sum(item => item.Price * item.Quantity);

			if (coupon.MinimumOrderAmount.HasValue && orderTotal < coupon.MinimumOrderAmount.Value)
			{
				cart.TotalDiscount = 0;
				Session["Cart"] = cart;
				return Json(new
				{
					success = false,
					message = $"Mã giảm giá chỉ áp dụng cho đơn hàng có tổng giá trị từ {coupon.MinimumOrderAmount.Value:C} trở lên!",
					totalDiscount = 0
				});
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

			cart.CouponCode = coupon.Code;
			cart.TotalDiscount = totalDiscount;
			Session["Cart"] = cart;

			return Json(new { success = true, message = "Áp dụng mã giảm giá thành công!", totalDiscount = totalDiscount }, JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult ApplyCoupon(int couponId)
		{
			ShoppingCart cart = (ShoppingCart)Session["Cart"];
			if (cart == null || !cart.Items.Any())
			{
				return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" });
			}

			var coupon = db.Coupons.FirstOrDefault(c => c.Id == couponId && c.IsActive &&
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
				return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn!", totalDiscount = 0 }, JsonRequestBehavior.AllowGet);
			}
			if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
			{
				return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng." });
			}
			decimal orderTotal = cart.Items.Sum(item => item.Price * item.Quantity);

			if (coupon.MinimumOrderAmount.HasValue && orderTotal < coupon.MinimumOrderAmount.Value)
			{
				cart.TotalDiscount = 0;
				Session["Cart"] = cart;
				return Json(new
				{
					success = false,
					message = $"Mã giảm giá chỉ áp dụng cho đơn hàng có tổng giá trị từ {coupon.MinimumOrderAmount.Value:C} trở lên!",
					totalDiscount = 0
				});
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

			cart.CouponCode = coupon.Code;
			cart.TotalDiscount = totalDiscount;
			Session["Cart"] = cart;

			return Json(new { success = true, message = "Áp dụng mã giảm giá thành công!", totalDiscount = totalDiscount }, JsonRequestBehavior.AllowGet);
		}
		public JsonResult GetAvailableCoupons()
		{
			var coupons = db.Coupons
		.Where(c => c.IsActive && c.StartDate <= DateTime.Now && c.ExpirationDate >= DateTime.Now)
		.Select(c => new
		{
			c.Id,
			c.Code,
			c.Description,
			ExpirationDate = c.ExpirationDate
		})
		.ToList()
		.Select(c => new
		{
			c.Id,
			c.Code,
			c.Description,
			ExpirationDate = c.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ss")
		})
		.ToList();
			return Json(coupons, JsonRequestBehavior.AllowGet);
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
					// Tạo đơn hàng
					Models.EF.Order order = CreateOrder(req, cart);
					db.Orders.Add(order);
					db.SaveChanges();
					UpdateProductQuantities(cart, order);
					SendOrderEmails(order, req, cart);
					cart.ClearCart();

					// Xử lý thanh toán
					code = HandlePayment(req, order);
				}
			}
			return Json(code);
		}

		private Models.EF.Order CreateOrder(OrderViewModel req, ShoppingCart cart)
		{
			Models.EF.Order order = new Models.EF.Order
			{
				CustomerName = req.CustomerName,
				Phone = req.Phone,
				Address = req.Address,
				Email = req.Email,
				Status = 1,
				StatusPayMent = 1,
				CouponCode = cart.CouponCode,
				TotalDiscount = cart.TotalDiscount,
				Quantity = cart.GetTotalQuantity(),
				TypePayment = req.TypePayment,
				TypeShip = req.TypeShip,
				CreatedDate = DateTime.Now,
				ModifiedDate = DateTime.Now,
				CreatedBy = req.Phone,
				CustomerId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null,
				Code = GenerateOrderCode()
			};

			// Tính toán tổng tiền
			decimal thanhtien = cart.Items.Sum(x => x.Price * x.Quantity);
			order.TotalAmount = thanhtien + req.ShipCost - cart.TotalDiscount;

			// Thêm chi tiết đơn hàng
			foreach (var item in cart.Items)
			{
				order.OrderDetails.Add(new OrderDetail
				{
					ProductId = item.ProductId,
					Quantity = item.Quantity,
					Price = item.Price,
					Size = item.Size
				});
			}

			return order;
		}

		private string GenerateOrderCode()
		{
			Random rd = new Random();
			return "DH" +
				   rd.Next(0, 9).ToString() +
				   rd.Next(0, 9).ToString() +
				   rd.Next(0, 9).ToString() +
				   rd.Next(0, 9).ToString();
		}

		private void UpdateProductQuantities(ShoppingCart cart, Models.EF.Order order)
		{
			foreach (var item in cart.Items)
			{
				var product = db.Products
					.Include(p => p.ProductSize)
					.FirstOrDefault(p => p.Id == item.ProductId);

				if (product != null)
				{
					// Giảm số lượng sản phẩm
					product.ReduceQuantity(item.Quantity, item.Size);
					product.Quantity -= item.Quantity;
					product.SoldQuantity += item.Quantity;
					db.Entry(product).State = EntityState.Modified;
				}
			}
			db.SaveChanges();
		}

		private void SendOrderEmails(Models.EF.Order order, OrderViewModel req, ShoppingCart cart)
		{
			// Tạo HTML chi tiết sản phẩm
			string productDetailsHtml = GenerateProductDetailsHtml(cart);
			decimal thanhtien = cart.Items.Sum(x => x.Price * x.Quantity);
			decimal tongTien = (decimal)(thanhtien + req.ShipCost - order.TotalDiscount);
			SendCustomerEmail(order, req, productDetailsHtml, thanhtien, tongTien);
			SendAdminEmail(order, req, productDetailsHtml, thanhtien, tongTien);
		}

		private string GenerateProductDetailsHtml(ShoppingCart cart)
		{
			StringBuilder strSanPham = new StringBuilder();

			foreach (var sp in cart.Items)
			{
				strSanPham.Append("<tr>");
				strSanPham.Append($"<td style=\"border-bottom:1px solid #e8e8e8; border-collapse:collapse; padding:10px;\">{sp.ProductName}</td>");
				strSanPham.Append($"<td style=\"text-align:center;\">{FormatCurrency(sp.Price)}</td>");
				strSanPham.Append($"<td style=\"text-align:center;\">{sp.Quantity}</td>");
				strSanPham.Append($"<td style=\"text-align:center;\">{FormatCurrency(sp.TotalPrice)}</td>");
				strSanPham.Append("</tr>");
			}

			return strSanPham.ToString();
		}

		private void SendCustomerEmail(Models.EF.Order order, OrderViewModel req,
			string productDetailsHtml, decimal thanhtien, decimal tongTien)
		{
			// Đọc template email
			string contentCustomer = System.IO.File.ReadAllText(
				Server.MapPath("~/Content/templates/invoice-1.html"));
			contentCustomer = ReplaceEmailPlaceholders(
				contentCustomer,
				order,
				req,
				productDetailsHtml,
				thanhtien,
				tongTien
			);
			WebShoeShop.Common.Common.SendMail(
				"Double 2T-2Q Store",
				$"Đơn hàng #{order.Code}",
				contentCustomer,
				req.Email
			);
		}

		private void SendAdminEmail(Models.EF.Order order, OrderViewModel req,
			string productDetailsHtml, decimal thanhtien, decimal tongTien)
		{
			// Đọc template email
			string contentAdmin = System.IO.File.ReadAllText(
				Server.MapPath("~/Content/templates/send1.html"));
			contentAdmin = ReplaceEmailPlaceholders(
				contentAdmin,
				order,
				req,
				productDetailsHtml,
				thanhtien,
				tongTien
			);

			WebShoeShop.Common.Common.SendMail(
				"Double 2T-2Q Store",
				$"Đơn hàng mới #{order.Code}",
				contentAdmin,
				ConfigurationManager.AppSettings["EmailAdmin"]
			);
		}

		private string ReplaceEmailPlaceholders(string content, Models.EF.Order order,
			OrderViewModel req, string productDetailsHtml,
			decimal thanhtien, decimal tongTien)
		{
			return content
				.Replace("{{MaDon}}", order.Code)
				.Replace("{{SanPham}}", productDetailsHtml)
				.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
				.Replace("{{TenKhachHang}}", order.CustomerName)
				.Replace("{{Phone}}", order.Phone)
				.Replace("{{Email}}", req.Email)
				.Replace("{{DiaChiNhanHang}}", order.Address)
				.Replace("{{GiamGia}}", FormatCurrency((decimal)order.TotalDiscount))
				.Replace("{{ThanhTien}}", FormatCurrency(thanhtien))
				.Replace("{{PhiVanChuyen}}", FormatCurrency(req.ShipCost))
				.Replace("{{TongTien}}", FormatCurrency(tongTien));
		}

		private dynamic HandlePayment(OrderViewModel req, Models.EF.Order order)
		{
			// Mặc định
			var code = new { Success = true, Code = req.TypePayment, Url = "" };

			// Nếu là thanh toán online
			if (req.TypePayment == 2)
			{
				string orderCode = GenerateOrderCode();
				req.Code = orderCode;
				Session[$"TempOrder_{orderCode}"] = req;
				order.Status = (int?)OrderStatus.Pending;
				db.SaveChanges();

				var url = UrlPayment(req.TypePaymentVN, order.Code);
				code = new { Success = true, Code = req.TypePayment, Url = url };
			}

			return code;
		}

		private string FormatCurrency(decimal value)
		{
			return WebShoeShop.Common.Common.FormatNumber(value, 0);
		}

		[HttpPost]
		public ActionResult AddToCart(int id, int quantity, int size)
		{
			var code = new { Success = false, msg = "", code = -1, Count = 0, ProductName = "", ProductImg = "", Size = 0, Quantity = 0, TotalPrice = 0m };
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

				// Return the necessary info for modal display
				code = new
				{
					Success = true,
					msg = "Thêm sản phẩm vào giỏ hàng thành công!",
					code = 1,
					Count = cart.Items.Count,
					ProductName = item.ProductName,
					ProductImg = item.ProductImg,
					Size = item.Size,
					Quantity = item.Quantity,
					TotalPrice = (decimal)item.TotalPrice
				};
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
					var product = db.ProductSizes.FirstOrDefault(x => x.ProductId == id && x.Size == size);
					if (product != null && product.Quantity < quantity)
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
		public JsonResult UpdateSize(int id, int oldSize, int newSize)
		{
			var cart = (ShoppingCart)Session["Cart"];
			if (cart != null)
			{
				var currentCartItem = cart.Items.FirstOrDefault(p =>
				 p.ProductId == id && p.Size == oldSize);
				var productSizeInfo = db.ProductSizes
		  .FirstOrDefault(ps => ps.ProductId == id && ps.Size == newSize);

				var duplicateSize = cart.Items.Count(p =>
					p.ProductId == id && p.Size == newSize) > 0;

				if (duplicateSize)
				{
					return Json(new
					{
						success = false,
						message = "Size này đã tồn tại trong giỏ hàng. Vui lòng chọn size khác hoặc điều chỉnh số lượng"
					});
				}
				int currentQuantityInCart = currentCartItem.Quantity;
				int availableQuantityInStock = (int)productSizeInfo.Quantity;
				if (availableQuantityInStock < currentQuantityInCart)
				{
					return Json(new
					{
						success = false,
						message = $"Size {newSize} chỉ còn {availableQuantityInStock} sản phẩm. Vui lòng giảm số lượng."
					});
				}
				cart.UpdateSize(id, oldSize, newSize);
				Session["Cart"] = cart;

				return Json(new
				{
					success = true,
					message = "Cập nhật size thành công"
				});
			}

			return Json(new
			{
				success = false,
				message = "Không tìm thấy giỏ hàng."
			});
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
		public enum OrderStatus
		{
			Pending = 1,      // Chờ thanh toán
			Paid = 2,         // Đã thanh toán
			PaymentFailed = 3,// Thanh toán thất bại
			Shipping = 4,     // Đang vận chuyển
			Completed = 5,    // Hoàn thành
			Cancelled = 6     // Đã hủy
		}
		#endregion
	}
}