using ClosedXML.Excel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using WebShoeShop.Common;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;
using WebShoeShop.Models.ViewModels;

namespace WebShoeShop.Areas.Admin.Controllers
{
	[CustomAuthorize]
	public class OrderController : Controller
	{

		private ApplicationDbContext db = new ApplicationDbContext();
		public OrderViewModel _orderViewModel = new OrderViewModel();
		// GET: Admin/Order
		public ActionResult Index(string Searchtext, int? page, int? month, int? status)
		{
			var pageSize = 10;
			if (page == null)
			{
				page = 1;
			}
			IEnumerable<Order> items = db.Orders.OrderByDescending(x => x.Id);

			if (month.HasValue)
			{
				items = items.Where(x => x.CreatedDate.Month == month);
			}
			if (status.HasValue)
			{
				items = items.Where(x => x.Status == status);
			}
			if (!string.IsNullOrEmpty(Searchtext))
			{
				string searchTextLower = Searchtext.ToLowerInvariant();
				items = items.Where(x => x.Code.IndexOf(searchTextLower, StringComparison.OrdinalIgnoreCase) != -1
					|| x.CustomerName.IndexOf(searchTextLower, StringComparison.OrdinalIgnoreCase) != -1
					|| x.Phone.IndexOf(searchTextLower, StringComparison.OrdinalIgnoreCase) != -1
				);
			}

			var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
			items = items.ToPagedList(pageIndex, pageSize);
			ViewBag.PageSize = pageSize;
			ViewBag.Page = page;
			ViewBag.Month = month;

			return View(items);
		}



		public ActionResult View(int id)
		{
			var item = db.Orders.Find(id);
			return View(item);
		}

		public ActionResult Partial_SanPham(int id)
		{
			var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
			return PartialView(items);
		}

		[HttpPost]
		public ActionResult UpdateTT(int id, int trangthai, int thanhtoan)
		{
			var item = db.Orders.Find(id);
			if (item != null)
			{
				db.Orders.Attach(item);
				item.Status = trangthai;
				item.StatusPayMent = thanhtoan;
				db.Entry(item).Property(x => x.TypePayment).IsModified = true;
				db.SaveChanges();
				return Json(new { message = "Success", Success = true });
			}
			return Json(new { message = "Unsuccess", Success = false });
		}

		public void ThongKe(string fromDate, string toDate)
		{
			var query = from o in db.Orders
						join od in db.OrderDetails on o.Id equals od.OrderId
						join p in db.Products
						on od.ProductId equals p.Id
						select new
						{
							CreatedDate = o.CreatedDate,
							Quantity = od.Quantity,
							Price = od.Price,
							OriginalPrice = p.Price
						};
			if (!string.IsNullOrEmpty(fromDate))
			{
				DateTime start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
				query = query.Where(x => x.CreatedDate >= start);
			}
			if (!string.IsNullOrEmpty(toDate))
			{
				DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
				query = query.Where(x => x.CreatedDate < endDate);
			}
			var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(r => new
			{
				Date = r.Key.Value,
				TotalBuy = r.Sum(x => x.OriginalPrice * x.Quantity), // tổng giá bán
				TotalSell = r.Sum(x => x.Price * x.Quantity) // tổng giá mua
			}).Select(x => new RevenueStatisticViewModel
			{
				Date = x.Date,
				Benefit = x.TotalSell - x.TotalBuy,
				Revenues = x.TotalSell
			});
		}
		public ActionResult ExportExcel(int orderId)
		{
			var wb = new XLWorkbook();
			var ws = wb.Worksheets.Add("Invoice");

			var ls = _orderViewModel.GetOrderDetailForExcel(orderId);

			// Thêm tiêu đề "Hóa đơn bán hàng"
			var titleRange = ws.Range("A1:E1");
			titleRange.Merge().Value = "Hóa đơn bán hàng";
			titleRange.Style.Font.Bold = true;
			titleRange.Style.Font.FontSize = 18;
			titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
			titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			titleRange.Style.Border.OutsideBorderColor = XLColor.Black;

			// Lấy thông tin khách hàng từ danh sách ls
			var customerName = ls.FirstOrDefault()?.CustomerName;
			var customerAddress = ls.FirstOrDefault()?.Address;
			var customerPhone = ls.FirstOrDefault()?.Phone;
			var createDate = ls.FirstOrDefault()?.CreateDate.ToString();
			var oderId = ls.FirstOrDefault()?.OrderId;

			// Thêm thông tin khách hàng
			ws.Cell("A2").Value = "Mã hóa đơn:";
			ws.Cell("B2").Value = orderId;
			ws.Cell("A3").Value = "Ngày đặt:";
			ws.Cell("B3").Value = createDate;
			var customerInfoRange = ws.Range("A4:E4");
			customerInfoRange.Merge();
			customerInfoRange.Value = "Thông tin khách hàng";
			customerInfoRange.Style.Font.Bold = true;

			ws.Cell("A5").Value = "Tên khách hàng:";
			ws.Cell("A6").Value = "Địa chỉ:";
			ws.Cell("A7").Value = "Số điện thoại:";

			ws.Cell("B5").Value = customerName;
			ws.Cell("B6").Value = customerAddress;
			ws.Cell("B7").Value = customerPhone;

			// Thêm bảng thông tin sản phẩm
			var tableHeaderCell = ws.Range("A9:E9");
			tableHeaderCell.Style.Font.Bold = true;
			tableHeaderCell.Style.Fill.BackgroundColor = XLColor.LightGray;
			tableHeaderCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
			tableHeaderCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			tableHeaderCell.Style.Border.OutsideBorderColor = XLColor.Black;
			tableHeaderCell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
			tableHeaderCell.Style.Border.InsideBorderColor = XLColor.Black;
			ws.Cell("A9").Value = "STT";
			ws.Cell("B9").Value = "Tên sản phẩm";
			ws.Cell("C9").Value = "Số lượng";
			ws.Cell("D9").Value = "Đơn giá";
			ws.Cell("E9").Value = "Thành tiền";

			int row = 10;
			int stt = 1;

			foreach (var item in ls)
			{
				ws.Cell("A" + row).Value = stt;
				ws.Cell("B" + row).Value = item.ProductName;
				ws.Cell("C" + row).Value = item.Quantity;
				ws.Cell("D" + row).Value = item.Price;
				ws.Cell("E" + row).Value = item.Quantity * item.Price;
				row++;
				stt++;
			}

			var tableRange = ws.Range("A9:E" + (row - 1));

			// Đặt đường viền cho bảng thông tin sản phẩm
			tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			tableRange.Style.Border.OutsideBorderColor = XLColor.Black;
			tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
			tableRange.Style.Border.InsideBorderColor = XLColor.Black;

			// Tính tổng cộng
			var totalRow = ws.Row(row);
			totalRow.Style.Font.Bold = true;
			totalRow.Cells("A", "E").Style.Border.TopBorder = XLBorderStyleValues.Thin;
			totalRow.Cells("A", "E").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
			totalRow.Cell("D").Value = "Tổng cộng:";
			totalRow.Cell("E").FormulaA1 = "=SUM(E10:E" + (row - 1) + ")";
			totalRow.Cell("D").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
			totalRow.Cell("E").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

			ws.Columns().AdjustToContents();

			var nameFile = "Invoice_2024_" + DateTime.Now.Ticks + ".xlsx";
			var pathFile = Server.MapPath("~/Resources/ExportExcel/" + nameFile);
			wb.SaveAs(pathFile);

			return Json(nameFile, JsonRequestBehavior.AllowGet);
		}
	}
}