using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fido2Authentication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    DateRegistered = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Passkeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ManufacturerGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    SignCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CredentialId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passkeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Passkeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DateRegistered", "Email", "Name", "PasswordHash" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), "admin@system.com", "Admin", "$2a$11$hVQvIo3SS4pXq2P9OZN9FupoOPgsoQ6Q9fU9leAX0O6TIn/ruKF8i" });

            migrationBuilder.CreateIndex(
                name: "IX_Passkeys_UserId",
                table: "Passkeys",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Passkeys");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
