using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagment.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BadgeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IconPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Threshold = table.Column<int>(type: "int", nullable: true),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberBadges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    BadgeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    EarnedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AwardedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberBadges_AspNetUsers_AwardedByUserId",
                        column: x => x.AwardedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MemberBadges_BadgeDefinitions_BadgeDefinitionId",
                        column: x => x.BadgeDefinitionId,
                        principalTable: "BadgeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberBadges_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberBadges_AwardedByUserId",
                table: "MemberBadges",
                column: "AwardedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberBadges_BadgeDefinitionId",
                table: "MemberBadges",
                column: "BadgeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberBadges_MemberId_BadgeDefinitionId",
                table: "MemberBadges",
                columns: new[] { "MemberId", "BadgeDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberBadges");

            migrationBuilder.DropTable(
                name: "BadgeDefinitions");
        }
    }
}
