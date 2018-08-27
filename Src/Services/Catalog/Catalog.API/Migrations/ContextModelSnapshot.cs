﻿// <auto-generated />
using System;
using Catalog.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Catalog.API.Migrations
{
    [DbContext(typeof(Context))]
    partial class ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Catalog.API.Model.Brand", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("InActive");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Brands");
                });

            modelBuilder.Entity("Catalog.API.Model.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("InActive");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Catalog.API.Model.Item", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BrandId");

                    b.Property<Guid?>("BrandId1");

                    b.Property<string>("Description");

                    b.Property<bool>("InActive");

                    b.Property<string>("MetaData")
                        .IsRequired()
                        .HasMaxLength(4000);

                    b.Property<string>("Name");

                    b.Property<string>("PictureFileName");

                    b.HasKey("Id");

                    b.HasIndex("BrandId1");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Catalog.API.Model.ItemCategory", b =>
                {
                    b.Property<Guid>("ItemId");

                    b.Property<Guid>("CategoryId");

                    b.HasKey("ItemId", "CategoryId");

                    b.HasIndex("CategoryId");

                    b.ToTable("ItemCategories");
                });

            modelBuilder.Entity("Catalog.API.Model.Unit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("InActive");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Units");
                });

            modelBuilder.Entity("Catalog.API.Model.Variant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AvailableStock");

                    b.Property<decimal>("Cost");

                    b.Property<int>("Count");

                    b.Property<Guid>("ItemId");

                    b.Property<int>("MaxStockThreshold");

                    b.Property<string>("MetaData")
                        .IsRequired()
                        .HasMaxLength(4000);

                    b.Property<bool>("OnReorder");

                    b.Property<decimal>("Price");

                    b.Property<int>("RestockThreshold");

                    b.Property<string>("SKU");

                    b.Property<decimal>("SuggestPrice");

                    b.Property<string>("UPC");

                    b.Property<Guid>("UnitId");

                    b.Property<Guid>("VendorId");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.HasIndex("UnitId");

                    b.HasIndex("VendorId");

                    b.ToTable("Variant");
                });

            modelBuilder.Entity("Catalog.API.Model.Vendor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("InActive");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Vendors");
                });

            modelBuilder.Entity("Catalog.API.Model.Item", b =>
                {
                    b.HasOne("Catalog.API.Model.Brand", "Brand")
                        .WithMany()
                        .HasForeignKey("BrandId1");
                });

            modelBuilder.Entity("Catalog.API.Model.ItemCategory", b =>
                {
                    b.HasOne("Catalog.API.Model.Category", "Category")
                        .WithMany("ItemCategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Catalog.API.Model.Item", "Item")
                        .WithMany("ItemCategories")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Catalog.API.Model.Variant", b =>
                {
                    b.HasOne("Catalog.API.Model.Item")
                        .WithMany("Variants")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Catalog.API.Model.Unit", "Unit")
                        .WithMany()
                        .HasForeignKey("UnitId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Catalog.API.Model.Vendor", "Vendor")
                        .WithMany()
                        .HasForeignKey("VendorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
