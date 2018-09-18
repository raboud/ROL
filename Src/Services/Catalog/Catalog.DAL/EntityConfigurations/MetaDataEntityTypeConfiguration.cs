using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using ROL.Services.Catalog.Domain;
using System.Collections.Generic;

namespace ROL.Services.Catalog.DAL.EntityConfigurations
{
	class MetaDataEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : MetaDataEntity
	{
		virtual public void Configure(EntityTypeBuilder<TEntity> builder)
		{
			var converter = new ValueConverter<Dictionary<string, string>, string>(
				v => JsonConvert.SerializeObject(v, Formatting.None),
				v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

			builder.Property(i => i.MetaData)
				.HasConversion(converter)
				.HasMaxLength(4000)
				.IsRequired();
		}
	}
}
