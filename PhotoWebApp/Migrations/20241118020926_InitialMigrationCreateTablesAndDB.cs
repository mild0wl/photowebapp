using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhotoWebApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrationCreateTablesAndDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    photoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    photoTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommentMode = table.Column<bool>(type: "bit", nullable: false),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photo", x => x.photoId);
                    table.ForeignKey(
                        name: "FK_Photo_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhotoId = table.Column<int>(type: "int", nullable: false),
                    commentValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Flagged = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comment_Photo_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photo",
                        principalColumn: "photoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "Role", "Token", "TokenExpiry", "Username" },
                values: new object[,]
                {
                    { 1, "admin@photowebapp.com", 1, "abcdefgh", new DateTime(2016, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin" },
                    { 2, "ccuser@photowebapp.com", 2, "abcdefgh", new DateTime(2016, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "ccuser" }
                });

            migrationBuilder.InsertData(
                table: "Photo",
                columns: new[] { "photoId", "CommentMode", "DatePosted", "FilePath", "IsPublic", "LikesCount", "description", "photoTitle", "tags", "userId" },
                values: new object[,]
                {
                    { 1, true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, 200, "description 1", "Photo 1", "#tag1", 2 },
                    { 2, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, 10, "description 2", "Photo 2", "#tag2", 2 }
                });

            migrationBuilder.InsertData(
                table: "Comment",
                columns: new[] { "CommentId", "DatePosted", "Flagged", "PhotoId", "commentValue" },
                values: new object[,]
                {
                    { 1, new DateTime(2016, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, "Comment1" },
                    { 2, new DateTime(2016, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, "Comment2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_PhotoId",
                table: "Comment",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Photo_userId",
                table: "Photo",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
