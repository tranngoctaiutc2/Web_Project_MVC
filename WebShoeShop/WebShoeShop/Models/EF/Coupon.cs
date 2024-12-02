using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShoeShop.Models.EF
{
	[Table("tb_Coupon")]
	public class Coupon
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(50)]
		public string Code { get; set; }

		[StringLength(255)]
		public string Description { get; set; }

		public decimal? DiscountAmount { get; set; }

		public decimal? DiscountPercentage { get; set; }

		public decimal? MaxDiscountAmount { get; set; }

		public decimal? MinimumOrderAmount { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime ExpirationDate { get; set; }

		public bool IsActive { get; set; }

		public int? UsageLimit { get; set; }

		public int UsageCount { get; set; }

	}

}