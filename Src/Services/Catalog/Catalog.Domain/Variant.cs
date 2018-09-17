using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.Domain
{
	public class Variant : MetaDataEntity
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }
		public string Name { get; set; }

		public string PictureFileName { get; set; }
		public Guid ItemId { get; set; }

		[Column(TypeName = "Decimal(19,4)")]
		public decimal Price { get; set; }
		[Column(TypeName = "Decimal(19,4)")]
		public decimal Cost { get; set; }
		[Column(TypeName = "Decimal(19,4)")]
		public decimal SuggestPrice { get; set; }

		public Guid UnitId { get; set; }
		public Unit Unit { get; set; }
		public int Count { get; set; }

		public Guid VendorId { get; set; }
		public Vendor Vendor { get; set; }

		public string UPC { get; set; }
		public string SKU { get; set; }

		// Quantity in stock
		public int AvailableStock { get; set; }

		// Available stock at which we should reorder
		public int RestockThreshold { get; set; }

		// Maximum number of units that can be in-stock at any time (due to physicial/logistical constraints in warehouses)
		public int MaxStockThreshold { get; set; }

		/// <summary>
		/// True if item is on reorder
		/// </summary>
		public bool OnReorder { get; set; }

		public Variant()
		{
		}
	}
}
