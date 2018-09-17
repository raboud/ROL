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
			builder.ToTable("Unit");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
			   .IsRequired();

			builder.Property(cb => cb.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.HasIndex(v => v.Name)
				.IsUnique();
		}
	}

}
