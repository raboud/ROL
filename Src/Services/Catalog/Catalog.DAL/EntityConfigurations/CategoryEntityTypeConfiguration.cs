using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ROL.Services.Catalog.Domain;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
    class CategoryEntityTypeConfiguration
        : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
			builder.HasIndex(v => v.Name)
				.IsUnique();

			builder.HasMany(c => c.Children)
				.WithOne();

			builder.HasOne(c => c.Parent)
				.WithMany(p => p.Children)
				.HasForeignKey(c => c.ParentId);
		}
	}
}
