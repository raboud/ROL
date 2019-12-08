using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.Domain
{
	[Table("Item")]
	public class Item : MetaDataEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }
		public string Description { get; set; }
		public string PictureFileName { get; set; }
		public List<ItemCategory> ItemCategories { get; set; }
		public List<Variant> Variants { get; set; }
		public Guid BrandId { get; set; }
		public Brand Brand { get; set; }

		public bool InActive { get; set; }

		public Item()
		{
			ItemCategories = new List<ItemCategory>();
			Variants = new List<Variant>();
		}
	}
}
