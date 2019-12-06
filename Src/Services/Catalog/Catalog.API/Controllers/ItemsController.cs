using AutoMapper;
//using IntegrationEvents.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ROL.Services.Catalog.DAL;
//using Catalog.API.IntegrationEvents;
using ROL.Services.Catalog.Domain;
using ROL.Services.Catalog.DTO;
using ROL.Services.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.API.Controllers
{
	[Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class ItemsController : Controller
    {
        private readonly Context _context;
		private readonly ILogger<ItemsController> _logger;

		//		private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;
		private readonly IMapper _mapper;

		public ItemsController(
			Context context, 
			ILogger<ItemsController> logger,
//			ICatalogIntegrationEventService catalogIntegrationEventService,
			IMapper mapper)
        {
			this._context = context ?? throw new ArgumentNullException(nameof(context));
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

			//			_catalogIntegrationEventService = catalogIntegrationEventService ?? throw new ArgumentNullException(nameof(catalogIntegrationEventService));

			((DbContext)context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

			this._mapper = mapper;
		}

		// GET: api/Products
		[HttpGet]
		[ProducesResponseType(typeof(List<ItemDTO>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetItems(
			[FromQuery]string name,
			[FromQuery]Guid? typeId,
			[FromQuery]Guid? brandId
		)
        {
			IQueryable<Item> root = (IQueryable<Item>)_context.Items;

			if (!string.IsNullOrEmpty(name))
			{
				root = root.Where(p => p.Name.StartsWith(name));
			}

			if (typeId.HasValue)
			{
				root = root.Where(ci => ci.ItemCategories.Any(ct => ct.CategoryId == typeId));
			}

			if (brandId.HasValue)
			{
				root = root.Where(ci => ci.BrandId == brandId);
			}

			List<Item> items = await SetupIncludes(root
				.Where(i => !i.InActive))
				.OrderBy(c => c.Name)
				.ToListAsync();

			return Ok(_mapper.Map<List<ItemDTO>>(items));
		}

		// GET: api/v1/[controller]/Page
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedViewModel<Item>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page(
			[FromQuery]string name,
			[FromQuery]Guid? typeId,
			[FromQuery]Guid? brandId, 
			[FromQuery]int pageSize = 10, 
			[FromQuery]int pageIndex = 0)
		{
			IQueryable<Item> root = (IQueryable<Item>)_context.Items;

			if (!string.IsNullOrEmpty(name))
			{
				root = root.Where(p => p.Name.StartsWith(name));
			}

			if (typeId.HasValue)
			{
				root = root.Where(ci => ci.ItemCategories.Any(ct => ct.CategoryId == typeId));
			}

			if (brandId.HasValue)
			{
				root = root.Where(ci => ci.BrandId == brandId);
			}

			long totalItems = await root
				.LongCountAsync();

			List<Item> items = await SetupIncludes(root.Where(i => !i.InActive))
				.OrderBy(c => c.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			//			ChangeUriPlaceholder(items);

			PaginatedViewModel<ItemDTO> model = new PaginatedViewModel<ItemDTO>(
				pageIndex, pageSize, totalItems, _mapper.Map<List<ItemDTO>>(items));

			return Ok(model);
		}

		//private void ChangeUriPlaceholder(List<Product> items)
		//{
		//	var baseUri = _settings.PicBaseUrl;

		//	items.ForEach(catalogItem =>
		//	{
		//		ChangeUriPlaceholder(catalogItem);
		//	});
		//}

		//private void ChangeUriPlaceholder(Product item)
		//{
		//	var baseUri = _settings.PicBaseUrl;

		//		item.PictureUri = _settings.AzureStorageEnabled
		//			? baseUri + item.PictureFileName
		//			: baseUri.Replace("[0]", item.Id.ToString());
		//}

		private IQueryable<Item> SetupIncludes(IQueryable<Item> query)
		{
			return query
				.Include(i => i.Brand)
				.Include(i => i.ItemCategories)
					.ThenInclude(t => t.Category)
				.Include(i => i.Variants)
					.ThenInclude(v => v.Vendor)
				.Include(i => i.Variants)
					.ThenInclude(v => v.Unit);
		}

		// GET: api/Products/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(ItemDTO), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetProduct([FromRoute] Guid id)
        {
			if (id == null)
			{
				return BadRequest();
			}

			Item product = await SetupIncludes(_context.Items)
				.SingleOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }
			return Ok(_mapper.Map<ItemDTO>(product));
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
//		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutProduct([FromRoute] Guid id, [FromBody] ItemDTO product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

			Item p = await _context.Items
				.SingleOrDefaultAsync(i => i.Id == product.Id);

			if (p == null)
			{
				return NotFound(new { Message = $"Item with id {product.Id} not found." });
			}

//			bool raiseProductPriceChangedEvent = p.Price != product.Price;

			try
			{
				if (product.Types != null)
				{
					List<ItemCategory> toAdd = new List<ItemCategory>();
					List<ItemCategory> toDel = new List<ItemCategory>();
					List<Category> types = await _context.Categories.ToListAsync();
					List<ItemCategory> current = await _context
						.ItemCategories
						.Where(pc => pc.ItemId == id)
						.Include(pc => pc.Category)
						.ToListAsync();

					foreach (ItemCategory pc in current)
					{
						if (!product.Types.Contains(pc.Category.Name))
						{
							toDel.Add(pc);
						}
						else
						{
							product.Types.Remove(pc.Category.Name);
						}
					}

					foreach (string type in product.Types)
					{
						ItemCategory pc = new ItemCategory()
						{
							ItemId = product.Id,
							CategoryId = types.Where(t => t.Name == type).SingleOrDefault().Id
						};
						toAdd.Add(pc);
					}
					await _context.AddRangeAsync(toAdd);
					_context.RemoveRange(toDel);
				}

//				if (raiseProductPriceChangedEvent) // Save product's data and publish integration event through the Event Bus if price has changed
				{
					//Create Integration Event to be published through the Event Bus
//					ProductPriceChangedIntegrationEvent priceChangedEvent = new ProductPriceChangedIntegrationEvent(product.Id, product.Price, p.Price);

					// Achieving atomicity between original Catalog database operation and the IntegrationEventLog thanks to a local transaction
//					await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(priceChangedEvent);

					// Publish through the Event Bus and mark the saved event as published
//					await _catalogIntegrationEventService.PublishThroughEventBusAsync(priceChangedEvent);
				}

				
				try
				{
					Item pr = _mapper.Map<Item>(product);

					_context.Entry(pr).State = EntityState.Modified;
					await _context.SaveChangesAsync();
				}
				catch (Exception e)
				{
					this._logger.LogCritical(e, "Error in PutProduct");
				}
			}
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
//		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostProduct([FromBody] ItemDTO product)
        {
			Item p2 = _mapper.Map<Item>(product);

			QueryTrackingBehavior initialState = _context.ChangeTracker.QueryTrackingBehavior;
			_context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

			_context.Items.Add(p2);
            await _context.SaveChangesAsync();
			_context.ChangeTracker.QueryTrackingBehavior = initialState;

			return CreatedAtAction("GetProduct", new { id = p2.Id }, _mapper.Map<ItemDTO>(p2));
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
        {
			Item item = await _context.Items.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			item.InActive = true;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool ProductExists(Guid id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}