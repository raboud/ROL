using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using ROL.Services.Catalog.Domain;
using System.Collections.Generic;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
	class MetaDataEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : MetaDataEntity
	{
		virtual public void Configure(EntityTypeBuilder<TEntity> builder)
		{
			builder.Property(i => i.MetaData)
				.HasConversion(
					d => JsonConvert.SerializeObject(d, Formatting.None),
					s => JsonConvert.DeserializeObject<Dictionary<string, string>>(s)
				)
				.HasMaxLength(4000)
				.IsRequired();
		}
	}
}
