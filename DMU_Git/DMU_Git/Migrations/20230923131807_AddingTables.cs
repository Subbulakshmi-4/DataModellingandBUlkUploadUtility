using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMU_Git.Migrations
{
    /// <inheritdoc />
    public partial class AddingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EntityColumnListMetadataModels_EntityId",
                table: "EntityColumnListMetadataModels",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityColumnListMetadataModels_EntityListMetadataModels_Ent~",
                table: "EntityColumnListMetadataModels",
                column: "EntityId",
                principalTable: "EntityListMetadataModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityColumnListMetadataModels_EntityListMetadataModels_Ent~",
                table: "EntityColumnListMetadataModels");

            migrationBuilder.DropIndex(
                name: "IX_EntityColumnListMetadataModels_EntityId",
                table: "EntityColumnListMetadataModels");
        }
    }
}
