using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IllustratorService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "illustrators",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    specialty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    profile_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    website_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    instagram_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    twitter_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_available_for_work = table.Column<bool>(type: "boolean", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_illustrators", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_illustrators_created_at",
                table: "illustrators",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_illustrators_is_available",
                table: "illustrators",
                column: "is_available_for_work");

            migrationBuilder.CreateIndex(
                name: "idx_illustrators_is_published",
                table: "illustrators",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "idx_illustrators_specialty",
                table: "illustrators",
                column: "specialty");

            migrationBuilder.CreateIndex(
                name: "idx_illustrators_user_id",
                table: "illustrators",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "illustrators");
        }
    }
}
