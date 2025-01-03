﻿using System.Collections.Generic;
using System.Linq;

namespace WebShoeShop.Models
{
	public class ShoppingCart
	{
		public List<ShoppingCartItem> Items { get; set; }
		public string CouponCode { get; set; }
		public decimal TotalDiscount { get; set; }
		public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
		public ShoppingCart()
		{
			this.Items = new List<ShoppingCartItem>();
		}

		public void AddToCart(ShoppingCartItem item, int Quantity)
		{
			var checkExists = Items.FirstOrDefault(x => x.ProductId == item.ProductId && x.Size == item.Size);
			if (checkExists != null)
			{
				checkExists.Quantity += Quantity;
				checkExists.TotalPrice = checkExists.Price * checkExists.Quantity;
			}
			else
			{
				Items.Add(item);
			}
		}

		public void Remove(int id, int size)
		{
			var checkExits = Items.SingleOrDefault(x => x.ProductId == id && x.Size == size);
			if (checkExits != null)
			{
				Items.Remove(checkExits);
			}
		}
		public void UpdateQuantity(int id, int quantity, int size)
		{
			var checkExits = Items.FirstOrDefault(x => x.ProductId == id && x.Size == size);
			if (checkExits != null)
			{
				checkExits.Quantity = quantity;
				checkExits.TotalPrice = checkExits.Price * checkExits.Quantity;
			}
		}

		public void UpdateSize(int productId, int oldSize, int newSize)
		{
			var existingItemWithNewSize = Items.FirstOrDefault(p =>
			p.ProductId == productId && p.Size == newSize);

			if (existingItemWithNewSize != null)
			{
				var itemToUpdate = Items.FirstOrDefault(p =>
					p.ProductId == productId && p.Size == oldSize);

				if (itemToUpdate != null)
				{
					existingItemWithNewSize.Quantity += itemToUpdate.Quantity;
					Items.Remove(itemToUpdate);
				}
			}
			else
			{
				var cartItem = Items.FirstOrDefault(p =>
					p.ProductId == productId && p.Size == oldSize);

				if (cartItem != null)
				{
					cartItem.Size = newSize;
				}
			}
		}


		public decimal GetTotalPrice()
		{
			return Items.Sum(x => x.TotalPrice);
		}
		public int GetTotalQuantity()
		{
			return Items.Sum(x => x.Quantity);
		}

		public void ClearCart()
		{
			Items.Clear();
		}

	}

	public class ShoppingCartItem
	{
		public int CartItemId { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string Alias { get; set; }
		public string CategoryName { get; set; }
		public string ProductImg { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public decimal TotalPrice { get; set; }
		public int Size { get; set; }
		public int AvailableStock { get; set; }
		public List<int> AvailableSizes { get; set; } = new List<int>();

		public decimal Discount { get; set; }
		public string CouponCode { get; set; }
	}
}
