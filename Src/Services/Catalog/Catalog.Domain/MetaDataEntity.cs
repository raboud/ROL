using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ROL.Services.Catalog.Domain
{
	public class MetaDataEntity: IEquatable<MetaDataEntity>, ICloneable
	{
		public Dictionary<string, object> MetaData { get; set; } = new Dictionary<string, object>();
		public MetaDataEntity()
		{
		}

		public bool Equals(MetaDataEntity other)
		{
			throw new NotImplementedException();
		}

		public object Clone()
		{
			return JsonConvert.DeserializeObject<MetaDataEntity>(this.ToString());
		}
	}
}
