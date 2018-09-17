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
using ROL.Services.Common.API;
using AutoMapper;
using ROL.Services.Common.DTO;
using ROL.Services.Catalog.DTO;

namespace ROL.Services.Catalog.API.Controllers
{
    [Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class VendorsController : Controller
    {
        private readonly Context _context;
		private readonly IMapper _mapper;

		public VendorsController(Context context,
			IMapper mapper)
        {
            _context = context;
			_mapper = mapper;
		}

		// GET: api/Vendors
		[HttpGet]
		[ResponseCache(Duration = 3600)]
		[ProducesResponseType(typeof(List<VendorDTO>), (int)HttpStatusCode.OK)]
		public async  Task<IActionResult> GetVendors()
        {
			List<Vendor> items = await _context.Vendors
				.Where(b => !b.InActive)
				.OrderBy(b => b.Name)
				.ToListAsync();

			return Ok(_mapper.Map<List<VendorDTO>>(items));
		}

		// GET: api/v1/[controller]/Page
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedViewModel<VendorDTO>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page([FromQuery]bool? all, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
		{
			IQueryable<Vendor> query = _context.Vendors;
			if (!all.HasValue || all.Value == false)
			{
				query = query
				.Where(b => !b.InActive);
			}

			long totalItems = await query
				.LongCountAsync();

			List<Vendor> items = await query
				.OrderBy(b => b.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			PaginatedViewModel<VendorDTO> model = new PaginatedViewModel<VendorDTO>(
				pageIndex, pageSize, totalItems, _mapper.Map<List<VendorDTO>>(items));

			return Ok(model);
		}

		// GET: api/Vendors/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(Vendor), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetVendor([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

			Vendor vendor = await _context.Vendors.SingleOrDefaultAsync(m => m.Id == id);

            if (vendor == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<VendorDTO>(vendor));
        }

        // PUT: api/Vendors/5
        [HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutVendor([FromRoute] Guid id, [FromBody] VendorDTO vendor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != vendor.Id)
            {
                return BadRequest();
            }

            _context.Entry(_mapper.Map<Vendor>(vendor)).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorExists(id))
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

        // POST: api/Vendors
        [HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostVendor([FromBody] VendorDTO vendor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Vendors.Add(_mapper.Map<Vendor>(vendor));
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVendor", new { id = vendor.Id }, vendor);
        }

        // DELETE: api/Vendors/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteVendor([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

			Vendor item = await _context.Vendors.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			if (await _context.Variants.AnyAsync(p => p.VendorId == id))
			{
				item.InActive = true;
			}
			else
			{
				_context.Vendors.Remove(item);
			}
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool VendorExists(Guid id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }
    }
}