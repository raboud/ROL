using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.Domain
{
	[Table("Brand")]
	public class Brand
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id  { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }
		public bool InActive { get; set; }
	}
}
