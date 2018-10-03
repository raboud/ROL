using Newtonsoft.Json;
using ROL.Services.Catalog.DTO;
using ROL.Services.Common.DTO;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.Client
{
	public class CatalogClient : HttpClient
	{
		private readonly string _itemPage = "api/v1/Items/page";
		private readonly string _item = "api/v1/Item/";

		public CatalogClient(HttpMessageHandler handler) : base(handler) { }

		public CatalogClient() : base()
		{
		}

		public async Task<PaginatedViewModel<ItemDTO>> GetItemPage()
		{
			string data = await GetStringAsync(_itemPage);
			return JsonConvert.DeserializeObject<PaginatedViewModel<ItemDTO>>(data);
		}

		public async Task<HttpResponseMessage> AddItem(ItemDTO item)
		{
			StringContent content = new StringContent(JsonConvert.SerializeObject(item), UTF8Encoding.UTF8, "application/json");
			return await PostAsync(_item, content);
		}

		public async Task<HttpResponseMessage> UpdateItem(ItemDTO item)
		{
			StringContent content = new StringContent(JsonConvert.SerializeObject(item), UTF8Encoding.UTF8, "application/json");
			return await PutAsync(_item + $"{item.Id}", content);
		}

		public async Task<ItemDTO> GettemAsync(int id)
		{
			string data = await GetStringAsync(_item + $"/{id}");
			return JsonConvert.DeserializeObject<ItemDTO>(data);
		}

		public async Task<HttpResponseMessage> DeleteItem(ItemDTO item)
		{
			StringContent content = new StringContent(JsonConvert.SerializeObject(item), UTF8Encoding.UTF8, "application/json");
			return await DeleteAsync(_item + $"{item.Id}");
		}

		public async Task<HttpResponseMessage> DeleteItem(ItemDTO item, CancellationToken token)
		{
			StringContent content = new StringContent(JsonConvert.SerializeObject(item), UTF8Encoding.UTF8, "application/json");
			return await DeleteAsync(_item + $"{item.Id}", token);
		}

	}
}
