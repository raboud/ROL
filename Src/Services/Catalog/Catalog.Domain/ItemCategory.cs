using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.Domain
{
	public class ItemCategory
	{
		public Guid ItemId { get; set; }
		public Item Item { get; set; }

		public Guid CategoryId { get; set; }
		public Category Category { get; set; }
	}
}
