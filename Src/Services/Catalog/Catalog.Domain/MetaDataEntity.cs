using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ROL.Services.Catalog.Domain
{
	public class MetaDataEntity: IEquatable<MetaDataEntity>, ICloneable
	{
		virtual public Dictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>();
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
