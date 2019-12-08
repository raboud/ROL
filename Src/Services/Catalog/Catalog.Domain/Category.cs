using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.Domain
{
	[Table("Category")]
	public class Category
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		public List<ItemCategory> ItemCategories { get; set; }

		public bool InActive { get; set; }

		public Guid ParentId { get; set; }
		public Category Parent {get;set;}
		public List<Category> Children { get; set; }

		public Category()
		{
			Children = new List<Category>();
		}
	}
}
