using System;
using System.Collections.Generic;

namespace ROL.Services.Catalog.DTO
{
	public class VariantDTO
	{

		public Guid Id { get; set; }
		public string Name { get; set; }

		public string PictureFileName { get; set; }
		public string PictureUri { get; set; }

		public decimal Price { get; set; }
		public decimal Cost { get; set; }
		public decimal SuggestPrice { get; set; }

		public int Count { get; set; }
		public string Unit { get; set; }
		public string Vendor { get; set; }

		public string UPC { get; set; }
		public string SKU { get; set; }


		// Quantity in stock
		public int AvailableStock { get; set; }
		// Available stock at which we should reorder
		public int RestockThreshold { get; set; }


		// Maximum number of units that can be in-stock at any time (due to physicial/logistical constraints in warehouses)
		public int MaxStockThreshold { get; set; }

		public bool OnReorder { get; set; }
		public bool InActive { get; set; }
		public Dictionary<string, string> MetaData { get; set; }
	}

}