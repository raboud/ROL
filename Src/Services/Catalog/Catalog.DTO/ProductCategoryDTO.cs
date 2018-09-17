using System;

namespace ROL.Services.Catalog.DTO
{
	public class ProductCategoryDTO
	{
		public Guid ItemId { get; set; }
		public ItemDTO Item {get;set;}

		public Guid CategoryId { get; set; }
		public CategoryDTO Category { get; set; }
    }
}
