﻿using System;
using System.Collections.Generic;

namespace ROL.Services.Catalog.DTO
{
	public class ItemDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Brand { get; set; }
		public string PictureUri { get; set; }

        public bool OnReorder { get; set; }
		public bool InActive { get; set; }

		public List<string> Types { get; set; }
		public List<VariantDTO> Variants { get; set; }

		public Dictionary<string, string> MetaData { get; set; }

		public ItemDTO()
		{
			this.MetaData = new Dictionary<string, string>();
			this.Types = new List<string>();
			this.Variants = new List<VariantDTO>();
		}
	}

}