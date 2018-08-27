using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Model
{
	public class Item
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string PictureFileName { get; set; }
		public List<ItemCategory> ItemCategories { get; set; }
		public List<Variant> Variants { get; set; }
		public int BrandId { get; set; }
		public Brand Brand { get; set; }

		public bool InActive { get; set; }

//		[NotMapped]
		public Dictionary<string, string> MetaData { get; set; }
		//[Obsolete()]
		//internal string _metaString { get; set; }

		public Item()
		{
			this.ItemCategories = new List<ItemCategory>();
			this.MetaData = new Dictionary<string, string>();
		}
	}
}
