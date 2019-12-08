using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
			builder.Property(i => i.MetaData)
				.HasJsonConversion<Dictionary<string, object>>()
				.IsRequired();
		}
	}

	public static class PropertyBuilderExtensions
	{
		public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class, new()
		{
			propertyBuilder.Metadata.SetValueConverter(new ValueConverter<T, string>
			(
				v => JsonConvert.SerializeObject(v),
				v => JsonConvert.DeserializeObject<T>(v) ?? new T()
			));
			propertyBuilder.Metadata.SetValueComparer(new ValueComparer<T>
			(
				(l, r) => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(r),
				v => v == null ? 0 : JsonConvert.SerializeObject(v).GetHashCode(),
				v => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(v))
			));
			propertyBuilder.HasColumnType("jsonb");

			return propertyBuilder;
		}
	}
}
