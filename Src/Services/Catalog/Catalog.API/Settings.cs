using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.API
{
	public class Settings : ROL.Services.Catalog.DAL.Settings
	{
		public string PicBaseUrl { get; set; }

		public string EventBusConnection { get; set; }

		public bool AzureStorageEnabled { get; set; }
	}
}
