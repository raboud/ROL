using ROL.Services.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
	class VariantEntityTypeConfiguration
		: MetaDataEntityTypeConfiguration<Variant>
	{
		override public void Configure(EntityTypeBuilder<Variant> builder)
		{
			base.Configure(builder);
			builder.ToTable("Variant");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
				.IsRequired();

			builder.Property(ci => ci.Name)
				.IsRequired(true)
				.HasMaxLength(50);

			builder.Property(ci => ci.Price)
				.IsRequired(true);

			builder.Property(ci => ci.PictureFileName)
				.IsRequired(false);

			builder.HasOne(ci => ci.Vendor)
				.WithMany()
				.HasForeignKey(ci => ci.VendorId);

			builder.HasOne(ci => ci.Unit)
				.WithMany()
				.HasForeignKey(ci => ci.UnitId);

			builder.HasIndex(p => new { p.Name, p.Count, p.UnitId })
				.IsUnique();

			//            builder.HasOne(ci => ci.CatalogTypes)
			//                .WithMany()
			//                .HasForeignKey(ci => ci.CatalogTypeId);
		}
	}
}
