using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShoeShop.Models.EF
{
	[Table("tb_Brand")]
	public class Brand
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(250)]
		public string Description { get; set; }

		public bool IsActive { get; set; } = true;
	}
}