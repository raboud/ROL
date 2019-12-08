using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ROL.Services.Catalog.Domain;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
	class VendorEntityTypeConfiguration
		: IEntityTypeConfiguration<Vendor>
	{
		public void Configure(EntityTypeBuilder<Vendor> builder)
		{
			builder.HasIndex(v => v.Name)
				.IsUnique();
		}
	}

}
