﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PickNPlace.DataAccess;

namespace PickNPlace.Migrations
{
    [DbContext(typeof(HekaDbContext))]
    [Migration("20230901080848_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("PickNPlace.DataAccess.PalletRecipe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Explanation")
                        .HasColumnType("text");

                    b.Property<int>("PalletLength")
                        .HasColumnType("integer");

                    b.Property<int>("PalletWidth")
                        .HasColumnType("integer");

                    b.Property<string>("RecipeCode")
                        .HasColumnType("text");

                    b.Property<int>("TotalFloors")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("PalletRecipe");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletRecipeFloor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("Cols")
                        .HasColumnType("integer");

                    b.Property<int>("FloorNumber")
                        .HasColumnType("integer");

                    b.Property<int?>("PalletRecipeId")
                        .HasColumnType("integer");

                    b.Property<int?>("Rows")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PalletRecipeId");

                    b.ToTable("PalletRecipeFloor");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletRecipeFloorItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("Col")
                        .HasColumnType("integer");

                    b.Property<bool>("IsVertical")
                        .HasColumnType("boolean");

                    b.Property<int?>("ItemOrder")
                        .HasColumnType("integer");

                    b.Property<int?>("PalletRecipeFloorId")
                        .HasColumnType("integer");

                    b.Property<int?>("PalletRecipeId")
                        .HasColumnType("integer");

                    b.Property<int?>("Row")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PalletRecipeFloorId");

                    b.HasIndex("PalletRecipeId");

                    b.ToTable("PalletRecipeFloorItem");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletStateLive", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("CompletedBatchCount")
                        .HasColumnType("integer");

                    b.Property<bool?>("CurrentBatchIsCompleted")
                        .HasColumnType("boolean");

                    b.Property<bool?>("CurrentBatchIsStarted")
                        .HasColumnType("boolean");

                    b.Property<int>("CurrentBatchNo")
                        .HasColumnType("integer");

                    b.Property<bool?>("CurrentItemIsCompleted")
                        .HasColumnType("boolean");

                    b.Property<bool?>("CurrentItemIsStarted")
                        .HasColumnType("boolean");

                    b.Property<int>("CurrentItemNo")
                        .HasColumnType("integer");

                    b.Property<bool?>("IsDropable")
                        .HasColumnType("boolean");

                    b.Property<bool?>("IsPickable")
                        .HasColumnType("boolean");

                    b.Property<int?>("PalletNo")
                        .HasColumnType("integer");

                    b.Property<int?>("PalletRecipeId")
                        .HasColumnType("integer");

                    b.Property<int?>("PlaceRequestId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PalletRecipeId");

                    b.HasIndex("PlaceRequestId");

                    b.ToTable("PalletStateLive");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PlaceRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("BatchCount")
                        .HasColumnType("integer");

                    b.Property<string>("RecipeCode")
                        .HasColumnType("text");

                    b.Property<string>("RecipeName")
                        .HasColumnType("text");

                    b.Property<string>("RequestNo")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("PlaceRequest");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PlaceRequestItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("PiecesPerBatch")
                        .HasColumnType("integer");

                    b.Property<int?>("PlaceRequestId")
                        .HasColumnType("integer");

                    b.Property<int?>("RawMaterialId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PlaceRequestId");

                    b.HasIndex("RawMaterialId");

                    b.ToTable("PlaceRequestItem");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.RawMaterial", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ItemCode")
                        .HasColumnType("text");

                    b.Property<string>("ItemName")
                        .HasColumnType("text");

                    b.Property<decimal?>("ItemNetWeight")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable("RawMaterial");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletRecipeFloor", b =>
                {
                    b.HasOne("PickNPlace.DataAccess.PalletRecipe", "PalletRecipe")
                        .WithMany("Floors")
                        .HasForeignKey("PalletRecipeId");

                    b.Navigation("PalletRecipe");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletRecipeFloorItem", b =>
                {
                    b.HasOne("PickNPlace.DataAccess.PalletRecipeFloor", "PalletRecipeFloor")
                        .WithMany("Items")
                        .HasForeignKey("PalletRecipeFloorId");

                    b.HasOne("PickNPlace.DataAccess.PalletRecipe", "PalletRecipe")
                        .WithMany("Items")
                        .HasForeignKey("PalletRecipeId");

                    b.Navigation("PalletRecipe");

                    b.Navigation("PalletRecipeFloor");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletStateLive", b =>
                {
                    b.HasOne("PickNPlace.DataAccess.PalletRecipe", "PalletRecipe")
                        .WithMany()
                        .HasForeignKey("PalletRecipeId");

                    b.HasOne("PickNPlace.DataAccess.PlaceRequest", "PlaceRequest")
                        .WithMany()
                        .HasForeignKey("PlaceRequestId");

                    b.Navigation("PalletRecipe");

                    b.Navigation("PlaceRequest");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PlaceRequestItem", b =>
                {
                    b.HasOne("PickNPlace.DataAccess.PlaceRequest", "PlaceRequest")
                        .WithMany("Items")
                        .HasForeignKey("PlaceRequestId");

                    b.HasOne("PickNPlace.DataAccess.RawMaterial", "RawMaterial")
                        .WithMany()
                        .HasForeignKey("RawMaterialId");

                    b.Navigation("PlaceRequest");

                    b.Navigation("RawMaterial");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletRecipe", b =>
                {
                    b.Navigation("Floors");

                    b.Navigation("Items");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PalletRecipeFloor", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("PickNPlace.DataAccess.PlaceRequest", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
