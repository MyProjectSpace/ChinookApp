using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chinook.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPlaylistTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "PlaylistId",
                table: "Playlist",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Playlist",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_UserId",
                table: "Playlist",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlist_AspNetUsers_UserId",
                table: "Playlist",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlist_AspNetUsers_UserId",
                table: "Playlist");

            migrationBuilder.DropIndex(
                name: "IX_Playlist_UserId",
                table: "Playlist");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Playlist");

            migrationBuilder.AlterColumn<long>(
                name: "PlaylistId",
                table: "Playlist",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
