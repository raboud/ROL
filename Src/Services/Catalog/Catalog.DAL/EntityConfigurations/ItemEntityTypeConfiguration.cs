using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ROL.Services.Catalog.Domain;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
	class ItemEntityTypeConfiguration
        : MetaDataEntityTypeConfiguration<Item>
    {
        public override void Configure(EntityTypeBuilder<Item> builder)
        {
			base.Configure(builder);
            builder.ToTable("Item");

            builder.Property(ci => ci.Name)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Property(ci => ci.PictureFileName)
                .IsRequired(false);

            builder.HasOne(ci => ci.Brand)
                .WithMany()
                .HasForeignKey(ci => ci.BrandId);
		}
	}
}
