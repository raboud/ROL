using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ROL.Services.Catalog.Domain;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
	class BrandEntityTypeConfiguration
        : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
			builder.HasIndex(v => v.Name)
				.IsUnique();
		}
    }

}
