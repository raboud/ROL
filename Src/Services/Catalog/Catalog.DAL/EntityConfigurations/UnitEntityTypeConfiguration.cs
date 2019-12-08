using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ROL.Services.Catalog.Domain;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
	class UnitEntityTypeConfiguration
		: IEntityTypeConfiguration<Unit>
	{
		public void Configure(EntityTypeBuilder<Unit> builder)
		{
			builder.HasIndex(v => v.Name)
				.IsUnique();
		}
	}

}
