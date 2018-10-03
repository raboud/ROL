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
            builder.ToTable("Category");

            builder.HasKey(ci => ci.Id);

            builder.Property(cb => cb.Name)
                .IsRequired()
                .HasMaxLength(100);

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
