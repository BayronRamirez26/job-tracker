using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Applications.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "job_description",
                table: "job_applications",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "job_description",
                table: "job_applications");
        }
    }
}
