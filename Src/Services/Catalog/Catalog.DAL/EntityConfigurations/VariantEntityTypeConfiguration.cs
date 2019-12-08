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

			builder.Property(ci => ci.Price)
				.IsRequired(true);

			builder.Property(ci => ci.PictureFileName)
				.IsRequired(false);

			builder.HasIndex(p => new { p.Name, p.Count, p.UnitId })
				.IsUnique();
		}
	}
}
