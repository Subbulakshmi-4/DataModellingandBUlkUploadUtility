﻿// <auto-generated />
using System;
using DMU_Git.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DMU_Git.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230923131807_AddingTables")]
    partial class AddingTables
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DMU_Git.Models.EntityColumnListMetadataModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("ColumnPrimaryKey")
                        .HasColumnType("boolean");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Datatype")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DefaultValue")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EntityColumnName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("EntityId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsNullable")
                        .HasColumnType("boolean");

                    b.Property<int>("Length")
                        .HasColumnType("integer");

                    b.Property<int>("UpdatedBy")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("EntityColumnListMetadataModels");
                });

            modelBuilder.Entity("DMU_Git.Models.EntityListMetadataModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CreatedBy")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EntityName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UpdatedBy")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("EntityListMetadataModels");
                });

            modelBuilder.Entity("DMU_Git.Models.EntityColumnListMetadataModel", b =>
                {
                    b.HasOne("DMU_Git.Models.EntityListMetadataModel", null)
                        .WithMany("EntityColumnListMetadata")
                        .HasForeignKey("EntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DMU_Git.Models.EntityListMetadataModel", b =>
                {
                    b.Navigation("EntityColumnListMetadata");
                });
#pragma warning restore 612, 618
        }
    }
}
