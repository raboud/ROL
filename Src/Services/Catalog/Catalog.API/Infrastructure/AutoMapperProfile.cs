using AutoMapper;
using ROL.Services.Catalog.API;
using ROL.Services.Catalog.DAL;
using ROL.Services.Catalog.Domain;
using Microsoft.Extensions.Options;
using ROL.Services.Catalog.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.API.Infrastructure
{
    public class AutoMapperProfile: Profile
    {
		public AutoMapperProfile()
		{
			CreateMap<Brand, BrandDTO>().ReverseMap();
			CreateMap<Category, CategoryDTO>().ReverseMap();
			CreateMap<Item, ItemDTO>()
				.ForMember(p => p.Brand, opt => opt.MapFrom(src => src.Brand.Name))
//				.ForMember(p => p.PictureUri, opt => opt.ResolveUsing<PictureUriReslover>())
				.ForMember(p => p.Types, opt => opt.MapFrom(src => src.ItemCategories.Select(e => e.Category.Name).ToList()));
			CreateMap<ItemDTO, Item>()
				.ForMember(p => p.Brand, opt => opt.ResolveUsing<BrandReslover>());
			CreateMap<Unit, UnitDTO>().ReverseMap();
			CreateMap<Variant, VariantDTO>()
				.ForMember(p => p.Unit, opt => opt.MapFrom(src => src.Unit.Name))
				.ForMember(p => p.Vendor, opt => opt.MapFrom(src => src.Vendor.Name));
			CreateMap<VariantDTO, Variant>()
				.ForMember(p => p.Unit, opt => opt.ResolveUsing<UnitReslover>())
				.ForMember(p => p.Vendor, opt => opt.ResolveUsing<VendorReslover>());
			CreateMap<Vendor, VendorDTO>().ReverseMap();
		}
	}

	internal class PictureUriReslover : IValueResolver<Item, ItemDTO, string>
	{
		private readonly Settings _settings;

		public PictureUriReslover(IOptionsSnapshot<Settings> settings)
		{
			_settings = settings.Value;
		}

		public string Resolve(Item source, ItemDTO destination, string destMember, ResolutionContext context)
		{
			string baseUri = _settings.PicBaseUrl;

			return _settings.AzureStorageEnabled
				? baseUri + source.PictureFileName
				: baseUri.Replace("[0]", source.Id.ToString());
		}
	}

	internal class UnitReslover : IValueResolver<VariantDTO, Variant, Unit>
	{
		private readonly Context _context;

		public UnitReslover(Context context)
		{
			_context = context;
		}

		public Unit Resolve(VariantDTO source, Variant destination, Unit destMember, ResolutionContext context)
		{
			Unit item = _context.Units.FirstOrDefault(u => u.Name == source.Unit);
			destination.UnitId = item.Id;
			return null;
		}
	}

	internal class VendorReslover : IValueResolver<VariantDTO, Variant, Vendor>
	{
		private readonly Context _context;

		public VendorReslover(Context context)
		{
			_context = context;
		}

		public Vendor Resolve(VariantDTO source, Variant destination, Vendor destMember, ResolutionContext context)
		{
			Vendor item = _context.Vendors.FirstOrDefault(u => u.Name == source.Vendor);
			destination.VendorId = item.Id;
			return null;
		}
	}


	internal class BrandReslover : IValueResolver<ItemDTO, Item, Brand>
	{
		private readonly Context _context;

		public BrandReslover(Context context)
		{
			_context = context;
		}

		public Brand Resolve(ItemDTO source, Item destination, Brand destMember, ResolutionContext context)
		{
			Brand item = _context.Brands.FirstOrDefault(u => u.Name == source.Brand);
			destination.BrandId = item.Id;
			return null;
		}
	}
}
