using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profiles_user_id",
                table: "profiles",
                column: "user_id");

            // Preserve any profile built under the old single-profile model by carrying it over as a
            // named profile before the source column is dropped. gen_random_uuid() is built into PG 13+.
            migrationBuilder.Sql(
                """
                INSERT INTO profiles (id, user_id, name, content, created_at, updated_at)
                SELECT gen_random_uuid(), id, 'My profile', profile, now(), now()
                FROM users
                WHERE profile IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "profile",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.AddColumn<string>(
                name: "profile",
                table: "users",
                type: "jsonb",
                nullable: true);
        }
    }
}
