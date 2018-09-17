using System.Collections.Generic;

namespace ROL.Services.Catalog.Domain
{
	public class MetaDataEntity
	{
		public Dictionary<string, string> MetaData { get; set; }
		public MetaDataEntity()
		{
			MetaData = new Dictionary<string, string>();
		}
	}
}
