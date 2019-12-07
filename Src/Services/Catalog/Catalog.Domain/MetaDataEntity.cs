using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ROL.Services.Catalog.Domain
{

	public class MetaDataEntity
	{
		public Dictionary<string, object> MetaData { get; set; } = new Dictionary<string, object>();
	}
}
