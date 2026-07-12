using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagment.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutPlansTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberWorkoutPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    ExperienceLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberWorkoutPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberWorkoutPlans_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberWorkoutPlans_MemberId",
                table: "MemberWorkoutPlans",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberWorkoutPlans");
        }
    }
}
