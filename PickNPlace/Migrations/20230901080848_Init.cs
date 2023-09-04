using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PickNPlace.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PalletRecipe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeCode = table.Column<string>(type: "text", nullable: true),
                    PalletWidth = table.Column<int>(type: "integer", nullable: false),
                    PalletLength = table.Column<int>(type: "integer", nullable: false),
                    TotalFloors = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletRecipe", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlaceRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestNo = table.Column<string>(type: "text", nullable: true),
                    BatchCount = table.Column<int>(type: "integer", nullable: false),
                    RecipeCode = table.Column<string>(type: "text", nullable: true),
                    RecipeName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "text", nullable: true),
                    ItemName = table.Column<string>(type: "text", nullable: true),
                    ItemNetWeight = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PalletRecipeFloor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PalletRecipeId = table.Column<int>(type: "integer", nullable: true),
                    FloorNumber = table.Column<int>(type: "integer", nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: true),
                    Cols = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletRecipeFloor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalletRecipeFloor_PalletRecipe_PalletRecipeId",
                        column: x => x.PalletRecipeId,
                        principalTable: "PalletRecipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PalletStateLive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PalletNo = table.Column<int>(type: "integer", nullable: true),
                    IsPickable = table.Column<bool>(type: "boolean", nullable: true),
                    IsDropable = table.Column<bool>(type: "boolean", nullable: true),
                    PlaceRequestId = table.Column<int>(type: "integer", nullable: true),
                    PalletRecipeId = table.Column<int>(type: "integer", nullable: true),
                    CurrentItemNo = table.Column<int>(type: "integer", nullable: false),
                    CurrentItemIsStarted = table.Column<bool>(type: "boolean", nullable: true),
                    CurrentItemIsCompleted = table.Column<bool>(type: "boolean", nullable: true),
                    CurrentBatchNo = table.Column<int>(type: "integer", nullable: false),
                    CurrentBatchIsStarted = table.Column<bool>(type: "boolean", nullable: true),
                    CurrentBatchIsCompleted = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedBatchCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletStateLive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalletStateLive_PalletRecipe_PalletRecipeId",
                        column: x => x.PalletRecipeId,
                        principalTable: "PalletRecipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PalletStateLive_PlaceRequest_PlaceRequestId",
                        column: x => x.PlaceRequestId,
                        principalTable: "PlaceRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaceRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaceRequestId = table.Column<int>(type: "integer", nullable: true),
                    RawMaterialId = table.Column<int>(type: "integer", nullable: true),
                    PiecesPerBatch = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaceRequestItem_PlaceRequest_PlaceRequestId",
                        column: x => x.PlaceRequestId,
                        principalTable: "PlaceRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaceRequestItem_RawMaterial_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "RawMaterial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PalletRecipeFloorItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PalletRecipeId = table.Column<int>(type: "integer", nullable: true),
                    PalletRecipeFloorId = table.Column<int>(type: "integer", nullable: true),
                    ItemOrder = table.Column<int>(type: "integer", nullable: true),
                    Row = table.Column<int>(type: "integer", nullable: true),
                    Col = table.Column<int>(type: "integer", nullable: true),
                    IsVertical = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletRecipeFloorItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalletRecipeFloorItem_PalletRecipe_PalletRecipeId",
                        column: x => x.PalletRecipeId,
                        principalTable: "PalletRecipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PalletRecipeFloorItem_PalletRecipeFloor_PalletRecipeFloorId",
                        column: x => x.PalletRecipeFloorId,
                        principalTable: "PalletRecipeFloor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PalletRecipeFloor_PalletRecipeId",
                table: "PalletRecipeFloor",
                column: "PalletRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletRecipeFloorItem_PalletRecipeFloorId",
                table: "PalletRecipeFloorItem",
                column: "PalletRecipeFloorId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletRecipeFloorItem_PalletRecipeId",
                table: "PalletRecipeFloorItem",
                column: "PalletRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletStateLive_PalletRecipeId",
                table: "PalletStateLive",
                column: "PalletRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletStateLive_PlaceRequestId",
                table: "PalletStateLive",
                column: "PlaceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceRequestItem_PlaceRequestId",
                table: "PlaceRequestItem",
                column: "PlaceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceRequestItem_RawMaterialId",
                table: "PlaceRequestItem",
                column: "RawMaterialId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PalletRecipeFloorItem");

            migrationBuilder.DropTable(
                name: "PalletStateLive");

            migrationBuilder.DropTable(
                name: "PlaceRequestItem");

            migrationBuilder.DropTable(
                name: "PalletRecipeFloor");

            migrationBuilder.DropTable(
                name: "PlaceRequest");

            migrationBuilder.DropTable(
                name: "RawMaterial");

            migrationBuilder.DropTable(
                name: "PalletRecipe");
        }
    }
}
