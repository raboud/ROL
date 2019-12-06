using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using HMS.Catalog.API.Infrastructure;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using ROL.Services.Common.API;
using ROL.Services.Catalog.DTO;
using AutoMapper;
using ROL.Services.Catalog.API.Infrastructure;
using ROL.Services.Common.DTO;
using ROL.Services.Catalog.Domain;
using ROL.Services.Catalog.DAL;

namespace ROL.Services.Catalog.API.Controllers
{
	[Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class BrandsController : Controller
	{
		private readonly Context _context;
		private readonly IMapper _mapper;

		public BrandsController(Context context,
			IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		// GET: api/v1/[controller]
		[HttpGet]
		[ProducesResponseType(typeof(List<BrandDTO>), (int)HttpStatusCode.OK)]
		[ResponseCache(Duration = 3600)]
		public async Task<IActionResult> GetItems([FromQuery]bool? all)
		{
			IQueryable<Brand> query;
			if (all.HasValue && all.Value == true)
			{
				query = _context.Brands;
			}
			else
			{
				query = _context.Brands
				.Where(b => !b.InActive);
			}
			List<Brand> items = await query
				.OrderBy(b => b.Name)
				.ToListAsync();

			return Ok(_mapper.Map<List<BrandDTO>>(items));
		}

		// GET: api/v1/[controller]/Page
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedViewModel<BrandDTO>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page([FromQuery]bool? all, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
		{
			IQueryable<Brand> query = _context.Brands;
			if (!all.HasValue || all.Value == false)
			{
				query = query
				.Where(b => !b.InActive);
			}

			long totalItems = await query
				.LongCountAsync();

			List<Brand> items = await query
				.OrderBy(b => b.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			PaginatedViewModel<BrandDTO> model = new PaginatedViewModel<BrandDTO>(
				pageIndex, pageSize, totalItems, _mapper.Map<List<BrandDTO>>(items));

			return Ok(model);
		}

		// GET: api/v1/[controller]/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(BrandDTO), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetBrand([FromRoute] Guid id)
		{
			Brand brand = await _context.Brands.SingleOrDefaultAsync(m => m.Id == id);

			if (brand == null)
			{
				return NotFound();
			}

			return Ok(_mapper.Map<BrandDTO>(brand));
		}

		// PUT: api/v1/[controller]/5
		[HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutBrand([FromRoute] Guid id, [FromBody] BrandDTO brand)
		{
			if (id != brand.Id)
			{
				return BadRequest();
			}

			_context.Entry(_mapper.Map<Brand>(brand)).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!Exists(id))
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

		// POST: api/v1/[controller]
		[HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostBrand([FromBody] BrandDTO brand)
		{
			_context.Brands.Add(_mapper.Map<Brand>(brand));
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetBrand", new { id = brand.Id }, brand);
		}

		// DELETE: api/Brands/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteBrand([FromRoute] Guid id)
		{
			Brand item = await _context.Brands.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			if (await _context.Items.AnyAsync(p => p.BrandId == id))
			{
				item.InActive = true;
			}
			else
			{
				_context.Brands.Remove(item);
			}
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool Exists(Guid id)
		{
			return _context.Brands.Any(e => e.Id == id);
		}
	}
}