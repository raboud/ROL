using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ROL.Services.Catalog.DAL;
using ROL.Services.Catalog.Domain;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using ROL.Services.Common.DTO;
using ROL.Services.Catalog.DTO;

namespace ROL.Services.Catalog.API.Controllers
{
    [Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class UnitsController : Controller
    {
        private readonly Context _context;
		private readonly IMapper _mapper;

		public UnitsController(Context context,
			IMapper mapper)
        {
            _context = context;
			_mapper = mapper;
		}

		// GET: api/Units
		[HttpGet]
		[ProducesResponseType(typeof(List<UnitDTO>), (int)HttpStatusCode.OK)]
		[ResponseCache(Duration = 3600)]
		public async Task<IActionResult> GetItems()
        {
			List<Unit> items = await _context.Units
				.Where(b => !b.InActive)
				.OrderBy(b => b.Name)
				.ToListAsync();

			return Ok(_mapper.Map<List<UnitDTO>>(items));
		}

		// GET: api/Units
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedViewModel<Unit>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page([FromQuery]bool? all, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
		{
			IQueryable<Unit> query = _context.Units;
			if (!all.HasValue || all.Value == false)
			{
				query = query
				.Where(b => !b.InActive);
			}

			long totalItems = await query
				.LongCountAsync();

			List<Unit> items = await query
				.OrderBy(b => b.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			PaginatedViewModel<UnitDTO> model = new PaginatedViewModel<UnitDTO>(
				pageIndex, pageSize, totalItems, _mapper.Map<List<UnitDTO>>(items));

			return Ok(model);
		}

		// GET: api/Units/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(UnitDTO), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetUnit([FromRoute] Guid id)
        {
			Unit item = await _context.Units.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			return Ok(_mapper.Map<UnitDTO>(item));
		}

		// PUT: api/Units/5
		[HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutUnit([FromRoute] Guid id, [FromBody] UnitDTO item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

			_context.Entry(_mapper.Map<Unit>(item)).State = EntityState.Modified;
			try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnitExists(id))
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

        // POST: api/Units
        [HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostUnit([FromBody] UnitDTO unit)
        {
            _context.Units.Add(_mapper.Map<Unit>(unit));
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnit", new { id = unit.Id }, unit);
        }

        // DELETE: api/Units/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteUnit([FromRoute] Guid id)
        {
			Unit item = await _context.Units.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			if (await _context.Variants.AnyAsync(p => p.UnitId == id))
			{
				item.InActive = true;
			}
			else
			{
				_context.Units.Remove(item);
			}
			await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UnitExists(Guid id)
        {
            return _context.Units.Any(e => e.Id == id);
        }
    }
}