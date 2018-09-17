using System;

namespace ROL.Services.Catalog.DTO
{
	public class CategoryDTO
    {
		public Guid Id { get; set; }
        public string Name { get; set; }
		public bool InActive { get; set; }
	}
}
