using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Decorations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryDisplayModeAndFeaturedImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "MediaAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowAsGrid",
                table: "GalleryItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "ShowAsGrid",
                table: "GalleryItems");
        }
    }
}
