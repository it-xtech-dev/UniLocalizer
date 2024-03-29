﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UniLocalizer.Demo.Models;

namespace UniLocalizer.Demo.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20191202223335_DbTranslations6")]
    partial class DbTranslations6
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("UniLocalizer.Demo.Models.Book", b =>
                {
                    b.Property<int>("BookId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Author");

                    b.Property<string>("OriginalTitle");

                    b.HasKey("BookId");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("UniLocalizer.Demo.Models.BookTranslation", b =>
                {
                    b.Property<Guid>("TranslationEntryGuid")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BookId");

                    b.Property<string>("CultureName")
                        .HasColumnType("varchar(5)");

                    b.Property<string>("Title");

                    b.HasKey("TranslationEntryGuid");

                    b.HasIndex("BookId");

                    b.ToTable("BookTranslation");
                });

            modelBuilder.Entity("UniLocalizer.Demo.Models.LocalizerResourceItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CultureName")
                        .IsRequired()
                        .HasColumnType("varchar(5)");

                    b.Property<string>("LocationKey")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<string>("ResourceKey")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("varchar(300)");

                    b.HasKey("Id");

                    b.ToTable("LocalizerResourceItem");
                });

            modelBuilder.Entity("UniLocalizer.Demo.Models.BookTranslation", b =>
                {
                    b.HasOne("UniLocalizer.Demo.Models.Book", "Book")
                        .WithMany("Translations")
                        .HasForeignKey("BookId");
                });
#pragma warning restore 612, 618
        }
    }
}
