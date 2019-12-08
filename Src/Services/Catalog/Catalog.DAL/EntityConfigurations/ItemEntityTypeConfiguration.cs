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
            builder.Property(ci => ci.PictureFileName)
                .IsRequired(false);

            base.Configure(builder);
		}
	}
}
