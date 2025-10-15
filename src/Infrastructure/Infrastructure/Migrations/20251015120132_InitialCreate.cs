using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying", nullable: true),
                    file_path = table.Column<string>(type: "character varying", nullable: true),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    sub_title_file_path = table.Column<string>(type: "character varying", nullable: true),
                    image = table.Column<string>(type: "character varying", nullable: true),
                    hash_value = table.Column<string>(type: "character varying", nullable: true),
                    release_date = table.Column<int>(type: "integer", nullable: true),
                    jellyfin_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("movies_pk_id", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movies");
        }
    }
}
